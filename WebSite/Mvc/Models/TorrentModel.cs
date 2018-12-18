namespace SitefinityWebApp.Mvc.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Web;

    public class TorrentModel
    {
        [Display(Name ="Title")]
        [Required(ErrorMessage = "The Field is required")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Additional Info")]
        public string AdditionalInfo { get; set; }

        [Display(Name = "Created On")]
        [Required(ErrorMessage = "The Field is required")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Genre")]
        [Required(ErrorMessage = "The Field is required")]
        public string Genre { get; set; }

        [Display(Name = "Title Image")]
        [Required(ErrorMessage = "The Field is required")]
        public HttpPostedFileBase Image { get; set; }

        [Display(Name = "Torrent File")]
        [Required(ErrorMessage = "The Field is required")]
        public HttpPostedFileBase File { get; set; }
    }
}