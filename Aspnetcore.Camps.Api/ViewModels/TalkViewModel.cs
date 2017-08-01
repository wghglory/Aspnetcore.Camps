using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aspnetcore.Camps.Api.ViewModels
{
    public class TalkViewModel
    {
        public string Url { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Abstract { get; set; }

        [Required]
        public string Category { get; set; }

        public string Level { get; set; }
        public string Prerequisites { get; set; }
        public DateTime StartingTime { get; set; } = DateTime.Now;
        public string Room { get; set; }

        // 如果要返回一些api patch/put等接口地址
        public ICollection<LinkModel> Links { get; set; }
    }
}