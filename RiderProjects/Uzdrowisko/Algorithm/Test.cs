﻿using System.Collections.ObjectModel;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Algorithm;

using System;

public class Test {
	void Main() { // make static to run
		Scheduler sch = new Scheduler(24*60,30,8*60,16*60);
		List<Appointment> appointments= new List<Appointment>();
		
		List<Room> rooms = new List<Room>();
		Room room = new Room() {Number="1"};
		room.Id = 1;
		room.Name = "R1";
		room.Capacity = 2;
		rooms.Add(room);
		room = new Room() {Number="2"};
		room.Id = 2;
		room.Name = "R2";
		room.Capacity = 1;
		rooms.Add(room);
		
		List<Employee> employees = new List<Employee>();
		Employee employee = new Employee() {Name="Flip1", Surname="Flippin",
			Email="flippin2@agh.edu.pl", PasswordHash = "fl11pp1n2", Role="Tt1", Active=true};
		employee.Id = 1;
		employees.Add(employee);
		employee = new Employee() {Name="Flip2", Surname="Flippin",
			Email="flippin1@agh.edu.pl", PasswordHash = "fl11pp1n1", Role="Tt1", Active=true};
		employee.Id = 2;
		employees.Add(employee);
		
		List<Treatment> treatments = new List<Treatment>();
		Treatment treatment = new Treatment() {Name="T1"};
		treatment.Id = 1;
		treatment.DurationMinutes = 90;
		//treatment.Flag = "0_S_1";
		treatment.Flag = null;
		treatment.Type = "Tt1";
		treatment.Rooms = new Collection<Room>();
		treatment.Rooms.Add(room);
		treatments.Add(treatment);
		
		rooms[0].TreatmentId=treatment.Id;
		rooms[0].Treatment=treatment;
		rooms[1].TreatmentId=treatment.Id;
		rooms[1].Treatment=treatment;
		
		Schedule cSchedule = new Schedule();
		cSchedule.Id = 1;
		cSchedule.Stay=new Stay();
		cSchedule.Stay.Id = 1;
		cSchedule.Stay.DateFrom = new DateOnly(2026, 1, 1);
		cSchedule.Stay.DateTo = new DateOnly(2026, 1, 14);
		cSchedule.Stay.Patient = new Patient() {Name="Flap", Surname="Flappin", PasswordHash = "fl4pp1n", Pesel = "123456"};
		cSchedule.Stay.Patient.Id = 1;
		cSchedule.Stay.PatientId = 1;
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

		Console.WriteLine("\nSchedule id "+appointments[0].ScheduleId);
		Console.WriteLine("Room: "+appointments[0].Room.Name);
		Console.WriteLine("Treatment:"+appointments[0].Treatment.Name);
		Console.WriteLine("Employee:"+appointments[0].Employee.Name);
		Console.WriteLine("Appointment date:"+appointments[0].Date);
		Console.WriteLine("Start at time block:"+appointments[0].TimeFrom);
		Console.WriteLine("End at time block:"+appointments[0].TimeTo);

		Schedule oldcSchedule = cSchedule;
		
		cSchedule = new Schedule();
		cSchedule.Id = 2;
		cSchedule.Stay=new Stay();
		cSchedule.Stay.Id = 2;
		cSchedule.Stay.DateFrom = new DateOnly(2026, 1, 1);
		cSchedule.Stay.DateTo = new DateOnly(2026, 1, 14);
		cSchedule.Stay.Patient = oldcSchedule.Stay.Patient;
		cSchedule.Stay.PatientId = oldcSchedule.Stay.PatientId;
		cSchedule.StayId = 2;
		
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

		Console.WriteLine("\nSchedule id "+appointments[0].ScheduleId);
		Console.WriteLine("Room: "+appointments[0].Room.Name);
		Console.WriteLine("Treatment:"+appointments[0].Treatment.Name);
		Console.WriteLine("Employee:"+appointments[0].Employee.Name);
		Console.WriteLine("Appointment date:"+appointments[0].Date);
		Console.WriteLine("Start at time block:"+appointments[0].TimeFrom);
		Console.WriteLine("End at time block:"+appointments[0].TimeTo);
	}
}