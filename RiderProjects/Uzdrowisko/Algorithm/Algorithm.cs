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
    
    public String failureReason = "No failure"; // debugging only - remove it once everything works!

    public static readonly TreatmentGroups treatmentGroups = new();

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

        if (endWorkMinutesOfDay <= startWorkHourMinutesOfDay)
            throw new ArgumentException("End hour must be greater than start hour.");

        blocksPerDay = (endWorkMinutesOfDay - startWorkHourMinutesOfDay) / timeBlockLengthMinutes;

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

    // private bool HasFlag(Treatment treatment, char flag) {
    //     return treatment.Flag.Contains(flag); // rider said it's never null (._.)
    // }
    //
    // private bool IsCategory(Treatment treatment, string category) {
    //     return string.Equals(
    //         treatment.Type,
    //         category,
    //         StringComparison.OrdinalIgnoreCase);
    // }

    // private bool IsThermotherapy(Treatment t)
    //     => IsCategory(t, "thermotherapy");
    //
    // private bool IsPhysicotherapy(Treatment t)
    //     => IsCategory(t, "physicotherapy");
    //
    // private bool IsElectrotherapy(Treatment t)
    //     => IsCategory(t, "electrotherapy");

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

    public static class TreatmentDependencyUtils
{
    public static bool TreatmentsConflict(
        Treatment a,
        Treatment b,
        DependencyGraph dependencyGraph,
        TreatmentGroups treatmentGroups)
    {
        if (a == b)
        {
            return true;
        }

        if (!dependencyGraph.Graph.ContainsKey(a) &&
            !dependencyGraph.Graph.ContainsKey(b))
        {
            return false;
        }

        if (dependencyGraph.Graph.ContainsKey(a))
        {
            foreach (TreatmentDependency dep in dependencyGraph.Graph[a])
            {
                if (dep.Type == DependencyType.CONFLICT ||
                    dep.Type == DependencyType.ORDER_SUCCESSION)
                {
                    if (dep.Target == "S")
                    {
                        if (dep.Id == b.Id)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (dep.Id == 0)
                        {
                            return true;
                        }

                        if (dep.Id > 0)
                        {
                            if (treatmentGroups.CategoryBasedGroups[dep.Id] == b.Type)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            foreach (Treatment treatment in treatmentGroups.CustomGroups[dep.Id].Treatments)
                            {
                                if (treatment == b)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (dependencyGraph.Graph.ContainsKey(b))
        {
            foreach (TreatmentDependency dep in dependencyGraph.Graph[b])
            {
                if (dep.Type == DependencyType.CONFLICT ||
                    dep.Type == DependencyType.ORDER_PRECEDENCE)
                {
                    if (dep.Target == "S")
                    {
                        if (dep.Id == a.Id)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (dep.Id == 0)
                        {
                            return true;
                        }

                        if (dep.Id > 0)
                        {
                            if (treatmentGroups.CategoryBasedGroups[dep.Id] == a.Type)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            foreach (Treatment treatment in treatmentGroups.CustomGroups[dep.Id].Treatments)
                            {
                                if (treatment == a)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
}
    
    // private bool TreatmentsConflict(Treatment a, Treatment b)
    // {
    //     // T: separation from thermotherapy.
    //     if (HasFlag(a, 'T') && IsThermotherapy(b))
    //         return true;
    //
    //     if (HasFlag(b, 'T') && IsThermotherapy(a))
    //         return true;
    //
    //     // P: separation from physicotherapy.
    //     if ((HasFlag(a, 'P') && IsPhysicotherapy(b)) ||
    //         (HasFlag(b, 'P') && IsPhysicotherapy(a)))
    //         return true;
    //
    //     // E: separation from electrotherapy.
    //     if ((HasFlag(a, 'E') && IsElectrotherapy(b)) ||
    //         (HasFlag(b, 'E') && IsElectrotherapy(a)))
    //         return true;
    //
    //     // S: separation between S-marked treatments.
    //     if (HasFlag(a, 'S') && HasFlag(b, 'S'))
    //         return true;
    //
    //     return false;
    // }

    private bool RespectsTreatmentConstraints(
        Treatment treatment,
        DateOnly day,
        int start,
        int end,
        List<Placement> placed,
        DependencyGraph dependencyGraph)
    {
        foreach (var other in placed)
        {
            if (day != other.day)
                continue;

            if (!TreatmentDependencyUtils.TreatmentsConflict(
                    treatment,
                    other.treatment,
                    dependencyGraph,
                    treatmentGroups))
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
        /*  Returns list of AppointmentLocal objects, which represent scheduled
            appointments for a patient.
        */
        // List<Schedule> schedules, // list of already existing schedules, unused
        List<AppointmentLocal> appointments, // list of already existing appointments
        List<Room> rooms, // available rooms
        List<Employee> personel, // available employees
        List<Treatment> treatments, // treatments we have to schedule for a patient
        Schedule cSchedule, // database entity object assigned for a patient
        List<BlockedTime> blocksTaken, // list of impossibilities (time slots when the patient is unavailable)
        PlanCostEvaluator? evaluator) // interface providing the function responsible for evaluating how good a plan is
    {
        blocksTaken ??= new List<BlockedTime>();
        DependencyGraph dependencyGraph = new DependencyGraph(treatments);

        // --------------------------------------------------------
        // 1. Find dates
        // --------------------------------------------------------

        List<DateOnly> days = new List<DateOnly>();

        for (DateOnly day = cSchedule.Stay.DateFrom; day <= cSchedule.Stay.DateTo; day = day.AddDays(1)) {
            days.Add(day);
        }

        if (days.Count == 0) {
            failureReason = "No available days!";
            return null;
        }

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
                length > blocksPerDay) {
                failureReason = "Can't allocate enough time for user ("+length.ToString()+" blocks)!";
                return null;
            }

            List<Room> availableRooms =
                CompatibleRooms(treatment, rooms);

            List<Employee> availablePersonnel =
                CompatiblePersonnel(treatment, personel);

            if (availableRooms.Count == 0 ||
                availablePersonnel.Count == 0) {
                failureReason = "No available personel!";
                return null;
            }

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

            if (possible.Count == 0) {
                failureReason = "No possible placement combinations!";
                return null;
            }
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
                        treatment, p.day, p.start, p.end, placed, dependencyGraph))
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

        if (bestPlacements == null) {
            failureReason = "No possible placement combinations (null)!";
            return null;
        }

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

        failureReason = "No failure";
        return result;
    }
}
