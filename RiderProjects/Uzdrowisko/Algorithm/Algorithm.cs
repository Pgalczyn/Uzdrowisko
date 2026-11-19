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

    public int getBlock(TimeOnly time) {
        int minutes = (int) time.ToTimeSpan().TotalMinutes;
        minutes -= startWorkHour;
		
        return (minutes+timeBlockLength-1)/timeBlockLength; // It's the same as Top(minutes/timeBlockLength)
    }

    public TimeOnly getTime(int block) {
        return TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(block * timeBlockLength + startWorkHour));
    }

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
    // Helpers
    // ------------------------------------------------------------

    private bool Overlaps(int aStart, int aEnd, int bStart, int bEnd) {
        return aStart < bEnd && bStart < aEnd;
    }

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
        Appointment appointment,
        int patientId) {
        return appointment.Schedule.Stay.PatientId == patientId;
    }

    // ------------------------------------------------------------
    // Resource compatibility
    // ------------------------------------------------------------

    private List<Room> CompatibleRooms(
        Treatment treatment,
        List<Room> rooms) {
        return rooms
            .Where(r => r.TreatmentId == treatment.Id && (r.Capacity == null || r.Capacity > 0))
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

    public static class TreatmentDependencyUtils {
        public static bool TreatmentsConflict(
            Treatment a,
            Treatment b,
            DependencyGraph dependencyGraph,
            TreatmentGroups treatmentGroups)
        {
            if (!dependencyGraph.Graph.ContainsKey(a) &&
                !dependencyGraph.Graph.ContainsKey(b))
            {
                return false;
            }

            if (dependencyGraph.Graph.ContainsKey(a)) {
                foreach (TreatmentDependency dep in dependencyGraph.Graph[a]) {
                    if (dep.Type == DependencyType.CONFLICT ||
                        dep.Type == DependencyType.ORDER_SUCCESSION) 
                    {
                        if (dep.Target == "S") {
                            if (dep.Id == b.Id) {
                                return true;
                            }
                        }
                        else {
                            if (dep.Id == 0) {
                                return true;
                            }

                            if (dep.Id > 0) {
                                if (treatmentGroups.CategoryBasedGroups[dep.Id] == b.Type) {
                                    return true;
                                }
                            }
                            else {
                                foreach (Treatment treatment in treatmentGroups.CustomGroups[dep.Id].Treatments) {
                                    if (treatment == b) {
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

    private bool RespectsTreatmentConstraints(
        Treatment treatment,
        DateOnly day,
        int start,
        int end,
        List<Appointment> placed,
        DependencyGraph dependencyGraph)
    {
        foreach (var other in placed)
        {
            if (day != other.Date)
                continue;

            if (!TreatmentDependencyUtils.TreatmentsConflict(
                    treatment,
                    other.Treatment,
                    dependencyGraph,
                    treatmentGroups))
                continue;

            int gap;

            if (start >= other.BlockTo)
                gap = start - other.BlockTo;
            else if (other.BlockFrom >= end)
                gap = other.BlockFrom - end;
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
        List<Appointment> appointments,
        List<Appointment> placed)
    {
        for (int block = start; block < end; block++)
        {
            int usage = 0;

            foreach (var a in appointments)
            {
                if (a.Date != day)
                    continue;

                if (a.Room.Id != room.Id)
                    continue;

                if (a.BlockFrom <= block && block < a.BlockTo)
                    usage++;
            }

            foreach (var p in placed)
            {
                if (p.Date != day)
                    continue;

                if (p.RoomId != room.Id)
                    continue;
                
                if (p.BlockFrom <= block && block < p.BlockFrom)
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
        List<Appointment> appointments,
        List<Appointment> placed)
    {
        foreach (var a in appointments) {
            if (a.Date != day)
                continue;

            if (a.EmployeeId != employee.Id)
                continue;

            if (!Overlaps(start, end, a.BlockFrom, a.BlockTo))
                continue;

            // Same employee can serve multiple customers
            // simultaneously in the same room.
            if (a.RoomId != room.Id)
                return false;
        }

        foreach (var p in placed) {
            if (p.Date != day)
                continue;

            if (p.EmployeeId != employee.Id)
                continue;

            if (!Overlaps(start, end, p.BlockFrom, p.BlockTo))
                continue;

            if (p.RoomId != room.Id)
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
        List<Appointment> customerAppointments,
        List<Appointment> placed)
    {
        foreach (var a in customerAppointments)
        {
            if (a.Date != day)
                continue;

            if (Overlaps(start, end, a.BlockFrom, a.BlockTo))
                return false;
        }

        foreach (var p in placed)
        {
            if (p.Date != day)
                continue;

            if (Overlaps(start, end, p.BlockFrom, p.BlockTo))
                return false;
        }

        return true;
    }

    // ------------------------------------------------------------
    // Main scheduling method
    // ------------------------------------------------------------

    public List<Appointment> schedule(
        /*  Returns list of Appointment objects, which represent scheduled
            appointments for a patient.
        */
        // List<Schedule> schedules, // list of already existing schedules, unused
        List<Appointment> appointments, // list of already existing appointments
        List<Room> rooms, // available rooms
        List<Employee> personel, // available employees
        List<Treatment> treatments, // treatments we have to schedule for a patient
        Schedule cSchedule, // database entity object assigned for a patient
        List<BlockedTime> blocksTaken, // list of impossibilities (time slots when the patient is unavailable)
        PlanCostEvaluator? evaluator) // interface providing the function responsible for evaluating how good a plan is
    {
        //blocksTaken ??= new List<BlockedTime>();
        //DependencyGraph dependencyGraph = new DependencyGraph(treatments); // I'm moving it to a different place so it includes pre-existing appointments as well

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

        List<Appointment> customerAppointments = appointments
            .Where(a => BelongsToCustomer(a, cSchedule.Stay.PatientId))
            .ToList();

        List<Treatment> allCustomerTreatments = treatments.Union(customerAppointments.Select(b => b.Treatment)).ToList();
        
        DependencyGraph dependencyGraph = new DependencyGraph(allCustomerTreatments); // moved to here!
        
        // --------------------------------------------------------
        // 3. Generate all possible placements
        // --------------------------------------------------------

        Dictionary<int, List<Appointment>> options = new();

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

            List<Appointment> possible = new();

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
                            possible.Add(new Appointment() {
                                //Id = 0,
                                Treatment = treatment,
                                TreatmentId = treatment.Id,
                                Date = day,
                                BlockFrom = start,
                                BlockTo  = end,
                                RoomId=room.Id,
                                Room=room,
                                Employee = employee,
                                EmployeeId = employee.Id,
                                Status = "Temprorary"
                            });
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

        List<Appointment> placed = new();

        List<Appointment> bestPlacements = null;
        double bestScore = double.PositiveInfinity;

        void Search(int index)
        {
            // All treatments have been assigned.
            if (index == orderedTreatments.Count)
            {
                List<Appointment> candidate = new();

                foreach (var p in placed) {
                    candidate.Add(p);
                    p.TimeFrom = getTime(p.BlockFrom);
                    p.TimeTo = getTime(p.BlockTo);
                }

                double score = (evaluator == null) ? (0) : (evaluator.evaluate(cSchedule, customerAppointments.Concat(candidate).ToList()));

                if (score < bestScore) {
                    bestScore = score;
                    bestPlacements = new List<Appointment>(placed);
                }

                return;
            }

            Treatment treatment = orderedTreatments[index];

            foreach (var p in options[treatment.Id])
            {
                if (!CustomerIsAvailable(
                    p.Date, p.BlockFrom, p.BlockTo,
                    customerAppointments, placed))
                    continue;

                if (!RoomHasCapacity(
                    p.Room, p.Date, p.BlockFrom, p.BlockTo,
                    appointments, placed))
                    continue;

                if (!EmployeeIsAvailable(
                    p.Employee, p.Room, p.Date,
                    p.BlockFrom, p.BlockTo, appointments, placed))
                    continue;

                if (!RespectsTreatmentConstraints(
                        treatment, p.Date, p.BlockFrom, p.BlockTo, customerAppointments.Concat(placed).ToList(), dependencyGraph))
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

        List<Appointment> result = new();

        foreach (var p in bestPlacements) {
            //p.Id=;
            p.Status = "Zaplanowane";
            p.ScheduleId = cSchedule.Id;
            p.Schedule = cSchedule;
            result.Add(p);
            cSchedule.Appointments.Add(p);
        }

        failureReason = "No failure";
        return result;
    }
}
