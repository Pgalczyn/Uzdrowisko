using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Algorithm;

public interface PlanCostEvaluator {
	public int evaluate(Schedule schedule, List<AppointmentLocal> newAppointments);
}