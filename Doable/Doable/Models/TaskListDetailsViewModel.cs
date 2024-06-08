using Doable.Models;

public class TaskListDetailsViewModel
{
    public Tasklist Tasklist { get; set; }
    public List<Docu> AdminDocus { get; set; }
    public List<Docu> EmployeeDocus { get; set; }
}
