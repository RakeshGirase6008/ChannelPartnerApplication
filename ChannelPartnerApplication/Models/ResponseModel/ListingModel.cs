namespace ChannelPartnerApplication.Models.ResponseModel
{
    public class ListingModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Rating { get; set; }
        public int TotalBoard { get; set; }
        public int TotalStandard { get; set; }
        public int TotalSubject { get; set; }
    }
}
