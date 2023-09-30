namespace MartenApp.Events;

public class MessageOutbox
{
	public Guid Id { get; set; }
	public string Content { get; set; }
	public DateTime CreateAt { get; set; }
}
