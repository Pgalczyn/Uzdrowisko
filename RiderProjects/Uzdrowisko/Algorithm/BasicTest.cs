using System.Collections.ObjectModel;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Algorithm;

using System;

public class BasicTest {
	static void Main(string[] args) {
		Scheduler sch = new Scheduler(24*60,30,8*60,16*60);
		List<AppointmentLocal> appointments= new List<AppointmentLocal>();
		
		List<Room> rooms = new List<Room>();
		Room room = new Room() {Number="1"};
		room.Id = 1;
		room.Name = "R1";
		room.Capacity = 1;
		rooms.Add(room);
		
		List<Employee> employees = new List<Employee>();
		Employee employee = new Employee() {Name="Flip", Surname="Flippin",
			Email="flippin@agh.edu.pl", PasswordHash = "fl11pp1n", Role="Tt1", Active=true};
		employee.Id = 1;
		employees.Add(employee);
		
		List<Treatment> treatments = new List<Treatment>();
		Treatment treatment = new Treatment() {Name="T1"};
		treatment.Id = 1;
		treatment.DurationMinutes = 90;
		treatment.Flag = "0_S_1";
		treatment.Type = "Tt1";
		treatment.Rooms = new Collection<Room>();
		treatment.Rooms.Add(room);
		treatments.Add(treatment);
		
		room.TreatmentId=treatment.Id;
		room.Treatment=treatment;
		
		Schedule cSchedule = new Schedule();
		cSchedule.Id = 1;
		cSchedule.Stay=new Stay();
		cSchedule.Stay.Id = 1;
		cSchedule.Stay.DateFrom = new DateOnly(2026, 1, 1);
		cSchedule.Stay.DateTo = new DateOnly(2026, 1, 14);
		cSchedule.Stay.Patient = new Patient() {Name="Flap", Surname="Flappin", PasswordHash = "fl4pp1n", Pesel = "123456"};
		cSchedule.Stay.Patient.Id = 1;
		cSchedule.Stay.PatientId = 1;
		cSchedule.Id = 1;
		cSchedule.StayId=1;
		
		List<BlockedTime> blocksTaken = new List<BlockedTime>();
		
		appointments = sch.schedule(appointments,
			rooms,
			employees,
			treatments,
			cSchedule,
			blocksTaken,
			null);

		if (appointments == null || appointments[0] == null) {
			Console.WriteLine("Schedule is empty (like my head)");
			Console.WriteLine(sch.failureReason);
			return;
		}

		Console.WriteLine("Room: "+appointments[0].room.Name);
		Console.WriteLine("Treatment:"+appointments[0].treatment.Name);
		Console.WriteLine("Employee:"+appointments[0].employee.Name);
		Console.WriteLine("Appointment date:"+appointments[0].date);
		Console.WriteLine("Start at time block:"+appointments[0].time_from);
		Console.WriteLine("End at time block:"+appointments[0].time_to);
		
		appointments = sch.schedule(appointments,
			rooms,
			employees,
			treatments,
			cSchedule,
			blocksTaken,
			null);

		if (appointments == null || appointments[0] == null) {
			Console.WriteLine("Schedule is empty (like my head)");
			Console.WriteLine(sch.failureReason);
			return;
		}

		Console.WriteLine("Room: "+appointments[0].room.Name);
		Console.WriteLine("Treatment:"+appointments[0].treatment.Name);
		Console.WriteLine("Employee:"+appointments[0].employee.Name);
		Console.WriteLine("Appointment date:"+appointments[0].date);
		Console.WriteLine("Start at time block:"+appointments[0].time_from);
		Console.WriteLine("End at time block:"+appointments[0].time_to);
	}
}