namespace GitAnomalyDetector.Models
{
    public class Payload
    {
        public EventAction? Action { get; set; }

        public Team? Team { get; set; }

        public Organization? Organization { get; set; }

        public User? Sender { get; set; }

        public Repository? Repository { get; set; }
    }
}
