using Doable.Models;

namespace Doable.Controllers
{
    public class TaskDetailsViewModel
    {
        public Tasklist Task { get; set; }
        public bool CanEdit { get; set; }
        public bool CanUploadFiles { get; set; }
    }
}
