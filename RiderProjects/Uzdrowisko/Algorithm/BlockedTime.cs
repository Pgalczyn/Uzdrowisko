namespace Algorithm;

public class BlockedTime {
	public DateOnly date { get; set; }
	public int start { get; set; }
	public int end { get; set; }

	public BlockedTime(DateOnly date, int start, int end)
	{
		this.date = date;
		this.start = start;
		this.end = end;
	}
}