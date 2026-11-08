namespace Algorithm;

using System;
using System.Collections.Generic;
using System.Linq;

using Uzdrowisko.Infrastructure.DataBase.Entities;

public class Scheduler {
    // Configuration
    public readonly int bufferTime;
    public readonly int timeBlockLength;
    public readonly int startWorkHour;
    public readonly int endWorkHour;

    public readonly int blocksPerDay;

    public Scheduler(
        int bufferTimeMinutes, // 1440 = 24*60 => 24h => different days
        int timeBlockLengthMinutes, // 30 => 30 minutes
        int startWorkHourMinutesOfDay, // 480 = 8*60 => 8 am
        int endWorkMinutesOfDay) // 960 = 16*60 => 4pm (16)
    {
        if (bufferTimeMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferTimeMinutes));

        if (timeBlockLengthMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeBlockLengthMinutes));

        if (endWorkHour <= startWorkHour)
            throw new ArgumentException("End hour must be greater than start hour.");

        int blockCount = (endWorkMinutesOfDay - startWorkHourMinutesOfDay) / timeBlockLengthMinutes;

        if (Math.Abs(blockCount - blockCount) > 1e-9)
            throw new ArgumentException("Working hours must be divisible by block length.");

        bufferTime = bufferTimeMinutes;
        timeBlockLength = timeBlockLengthMinutes;
        startWorkHour = startWorkHourMinutesOfDay;
        endWorkHour = endWorkMinutesOfDay;
    }

    // ------------------------------------------------------------
    // Internal placement representation
    // ------------------------------------------------------------

    private class Placement
    {
        public Treatment treatment;
        public DateOnly day;
        public int start;
        public int end;
        public Room room;
        public Employee employee;

        public Placement(
            Treatment treatment,
            DateOnly day,
            int start,
            int end,
            Room room,
            Employee employee)
        {
            this.treatment = treatment;
            this.day = day;
            this.start = start;
            this.end = end;
            this.room = room;
            this.employee = employee;
        }
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------

    private bool Overlaps(int aStart, int aEnd, int bStart, int bEnd) {
        return aStart < bEnd && bStart < aEnd;
    }

    private bool HasFlag(Treatment treatment, char flag) {
        return treatment.Flag.Contains(flag); // rider said it's never null (._.)
    }

    private bool IsCategory(Treatment treatment, string category) {
        return string.Equals(
            treatment.Type,
            category,
            StringComparison.OrdinalIgnoreCase);
    }

    private bool IsThermotherapy(Treatment t)
        => IsCategory(t, "thermotherapy");

    private bool IsPhysicotherapy(Treatment t)
        => IsCategory(t, "physicotherapy");

    private bool IsElectrotherapy(Treatment t)
        => IsCategory(t, "electrotherapy");

    private bool IsBlocked(
        DateOnly day,
        int start,
        int end,
        List<BlockedTime> blocksTaken)
    {
        foreach (var block in blocksTaken)
        {
            if (day==block.date &&
                Overlaps(start, end, block.start, block.end))
                return true;
        }

        return false;
    }

    // ------------------------------------------------------------
    // Customer identification
    // ------------------------------------------------------------

    private bool BelongsToCustomer(
        AppointmentLocal appointment,
        Schedule cSchedule)
    {
        if (appointment.schedule.Id == cSchedule.Id)
            return true;

        return appointment.schedule.Stay.PatientId == cSchedule.Stay.PatientId;
    }

    // ------------------------------------------------------------
    // Resource compatibility
    // ------------------------------------------------------------

    private List<Room> CompatibleRooms(
        Treatment treatment,
        List<Room> rooms)
    {
        return rooms
            .Where(r => r.Treatment.Id == treatment.Id && (r.Capacity == null || r.Capacity > 0))
            .ToList();
    }

    private List<Employee> CompatiblePersonnel(
        Treatment treatment,
        List<Employee> personel)
    {
        return personel
            .Where(p => p.Role == treatment.Type)
            .ToList();
    }

    // ------------------------------------------------------------
    // Treatment constraints
    // ------------------------------------------------------------

    private bool TreatmentsConflict(Treatment a, Treatment b)
    {
        // T: separation from thermotherapy.
        if (HasFlag(a, 'T') && IsThermotherapy(b))
            return true;

        if (HasFlag(b, 'T') && IsThermotherapy(a))
            return true;

        // P: separation from physicotherapy.
        if ((HasFlag(a, 'P') && IsPhysicotherapy(b)) ||
            (HasFlag(b, 'P') && IsPhysicotherapy(a)))
            return true;

        // E: separation from electrotherapy.
        if ((HasFlag(a, 'E') && IsElectrotherapy(b)) ||
            (HasFlag(b, 'E') && IsElectrotherapy(a)))
            return true;

        // S: separation between S-marked treatments.
        if (HasFlag(a, 'S') && HasFlag(b, 'S'))
            return true;

        return false;
    }

    private bool RespectsTreatmentConstraints(
        Treatment treatment,
        DateOnly day,
        int start,
        int end,
        List<Placement> placed)
    {
        foreach (var other in placed)
        {
            if (day != other.day)
                continue;

            if (!TreatmentsConflict(treatment, other.treatment))
                continue;

            int gap;

            if (start >= other.end)
                gap = start - other.end;
            else if (other.start >= end)
                gap = other.start - end;
            else
                gap = 0;

            if (gap < bufferTime)
                return false;
        }

        return true;
    }

    // ------------------------------------------------------------
    // Room capacity
    // ------------------------------------------------------------

    private bool RoomHasCapacity(
        Room room,
        DateOnly day,
        int start,
        int end,
        List<AppointmentLocal> appointments,
        List<Placement> placed)
    {
        for (int block = start; block < end; block++)
        {
            int usage = 0;

            foreach (var a in appointments)
            {
                if (a.date != day)
                    continue;

                if (a.room.Id != room.Id)
                    continue;

                if (a.time_from <= block && block < a.time_to)
                    usage++;
            }

            foreach (var p in placed)
            {
                if (p.day != day)
                    continue;

                if (p.room.Id != room.Id)
                    continue;
                
                if (p.start <= block && block < p.end)
                    usage++;
            }

            if (room.Capacity!=null && usage >= room.Capacity)
                return false;
        }

        return true;
    }

    // ------------------------------------------------------------
    // Employee availability
    // ------------------------------------------------------------

    private bool EmployeeIsAvailable(
        Employee employee,
        Room room,
        DateOnly day,
        int start,
        int end,
        List<AppointmentLocal> appointments,
        List<Placement> placed)
    {
        foreach (var a in appointments)
        {
            if (a.date != day)
                continue;

            if (a.employee.Id != employee.Id)
                continue;

            if (!Overlaps(start, end, a.time_from, a.time_to))
                continue;

            // Same employee can serve multiple customers
            // simultaneously in the same room.
            if (a.room.Id != room.Id)
                return false;
        }

        foreach (var p in placed)
        {
            if (p.day != day)
                continue;

            if (p.employee.Id != employee.Id)
                continue;

            if (!Overlaps(start, end, p.start, p.end))
                continue;

            if (p.room.Id != room.Id)
                return false;
        }

        return true;
    }

    // ------------------------------------------------------------
    // Customer availability
    // ------------------------------------------------------------

    private bool CustomerIsAvailable(
        DateOnly day,
        int start,
        int end,
        List<AppointmentLocal> customerAppointments,
        List<Placement> placed)
    {
        foreach (var a in customerAppointments)
        {
            if (a.date != day)
                continue;

            if (Overlaps(start, end, a.time_from, a.time_to))
                return false;
        }

        foreach (var p in placed)
        {
            if (p.day != day)
                continue;

            if (Overlaps(start, end, p.start, p.end))
                return false;
        }

        return true;
    }

    // ------------------------------------------------------------
    // Main scheduling method
    // ------------------------------------------------------------

    public List<AppointmentLocal> schedule(
        List<Schedule> schedules,
        List<AppointmentLocal> appointments,
        List<Room> rooms,
        List<Employee> personel,
        List<Treatment> treatments,
        Schedule cSchedule,
        List<BlockedTime> blocksTaken,
        PlanCostEvaluator evaluator)
    {
        blocksTaken ??= new List<BlockedTime>();

        // --------------------------------------------------------
        // 1. Find dates
        // --------------------------------------------------------

        List<DateOnly> days = new List<DateOnly>();

        for (DateOnly day = cSchedule.Stay.DateFrom; day <= cSchedule.Stay.DateTo; day = day.AddDays(1)) {
            days.Add(day);
        }

        if (days.Count == 0)
            return null;

        // --------------------------------------------------------
        // 2. Existing appointments of this customer
        // --------------------------------------------------------

        List<AppointmentLocal> customerAppointments = appointments
            .Where(a => BelongsToCustomer(a, cSchedule))
            .ToList();

        // --------------------------------------------------------
        // 3. Generate all possible placements
        // --------------------------------------------------------

        Dictionary<int, List<Placement>> options = new();

        foreach (var treatment in treatments) {
            int length = (treatment.DurationMinutes + timeBlockLength -1)/timeBlockLength; // It's the same as Top(duration/timeBlockLenth)
            if (length <= 0 ||
                length > blocksPerDay)
                return null;

            List<Room> availableRooms =
                CompatibleRooms(treatment, rooms);

            List<Employee> availablePersonnel =
                CompatiblePersonnel(treatment, personel);

            if (availableRooms.Count == 0 ||
                availablePersonnel.Count == 0)
                return null;

            List<Placement> possible = new();

            foreach (DateOnly day in days) {
                for (int start = 0; start <= blocksPerDay - length; start++)
                {
                    int end = start + length;

                    if (IsBlocked(day, start, end, blocksTaken))
                        continue;

                    foreach (var room in availableRooms)
                    {
                        foreach (var employee in availablePersonnel)
                        {
                            possible.Add(new Placement(
                                treatment,
                                day,
                                start,
                                end,
                                room,
                                employee));
                        }
                    }
                }
            }

            options[treatment.Id] = possible;

            if (possible.Count == 0)
                return null;
        }

        // --------------------------------------------------------
        // 4. Order treatments by number of possible placements
        // --------------------------------------------------------

        List<Treatment> orderedTreatments = treatments
            .OrderBy(t => options[t.Id].Count)
            .ToList();

        // --------------------------------------------------------
        // 5. Backtracking search
        // --------------------------------------------------------

        List<Placement> placed = new();

        List<Placement> bestPlacements = null;
        double bestScore = double.PositiveInfinity;

        void Search(int index)
        {
            // All treatments have been assigned.
            if (index == orderedTreatments.Count)
            {
                List<AppointmentLocal> candidate = new();

                int tempId = -1;

                foreach (var p in placed) {
                    candidate.Add(new AppointmentLocal
                    (
                        tempId--,
                        cSchedule,
                        p.treatment,
                        p.employee,
                        p.room,
                        p.day,
                        p.start,
                        p.end
                    ));
                }

                double score = (evaluator == null) ? (0) : (evaluator.evaluate(cSchedule, candidate));

                if (score < bestScore) {
                    bestScore = score;
                    bestPlacements = new List<Placement>(placed);
                }

                return;
            }

            Treatment treatment = orderedTreatments[index];

            foreach (var p in options[treatment.Id])
            {
                if (!CustomerIsAvailable(
                    p.day, p.start, p.end,
                    customerAppointments, placed))
                    continue;

                if (!RoomHasCapacity(
                    p.room, p.day, p.start, p.end,
                    appointments, placed))
                    continue;

                if (!EmployeeIsAvailable(
                    p.employee, p.room, p.day,
                    p.start, p.end, appointments, placed))
                    continue;

                if (!RespectsTreatmentConstraints(
                    treatment, p.day, p.start, p.end, placed))
                    continue;

                placed.Add(p);

                Search(index + 1);

                placed.RemoveAt(placed.Count - 1);
            }
        }

        Search(0);

        // --------------------------------------------------------
        // 6. No valid complete schedule
        // --------------------------------------------------------

        if (bestPlacements == null)
            return null;

        // --------------------------------------------------------
        // 7. Create final appointments
        // --------------------------------------------------------

        int nextId = appointments.Count == 0
            ? 0
            : appointments.Max(a => a.id) + 1;

        List<AppointmentLocal> result = new();

        foreach (var p in bestPlacements)
        {
            result.Add(new AppointmentLocal(
                nextId++,
                cSchedule,
                p.treatment,
                p.employee,
                p.room,
                p.day,
                p.start,
                p.end
            ));
        }

        return result;
    }
}
