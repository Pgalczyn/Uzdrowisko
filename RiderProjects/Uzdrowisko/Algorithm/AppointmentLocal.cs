using System.Diagnostics;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Algorithm;

public class AppointmentLocal {
	public int id;
	public Schedule schedule;
	public Treatment treatment;
	public Employee employee;
	public Room room;
	public DateOnly date;
	public int time_from; // number of time blocks
	public int time_to; // number of time blocks

	public AppointmentLocal(Scheduler scheduler, Appointment appointment) {
		id = appointment.Id;
		schedule = appointment.Schedule;
		treatment = appointment.Treatment;
		employee = appointment.Employee;
		room = appointment.Room;
		date = appointment.Date;
		
		int startMinutes = (int) appointment.TimeFrom.ToTimeSpan().TotalMinutes;
		int endMinutes = (int) appointment.TimeTo.ToTimeSpan().TotalMinutes;
		startMinutes -= scheduler.startWorkHour;
		endMinutes -= scheduler.startWorkHour;
		
		time_from = (startMinutes+scheduler.timeBlockLength-1)/scheduler.timeBlockLength; // It's the same as Top(startMinutes/timeBlockLength)
		time_to = (endMinutes+scheduler.timeBlockLength-1)/scheduler.timeBlockLength;
	}

	public AppointmentLocal(
		int id,
		Schedule schedule,
		Treatment treatment,
		Employee employee,
		Room room,
		DateOnly date,
		int time_from,
		int time_to)
	{
		this.id = id;
		this.schedule = schedule;
		this.treatment = treatment;
		this.employee = employee;
		this.room = room;
		this.date = date;
		this.time_from = time_from;
		this.time_to = time_to;
	}
}