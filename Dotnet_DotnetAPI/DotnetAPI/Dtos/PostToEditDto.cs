using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetAPI.Dtos
{
    public class PostToEditDto
    {
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public string PostContent { get; set; }

        public PostToEditDto()
        {
            if (PostTitle == null)
                PostTitle = "";

            if (PostContent == null)
                PostContent = "";
        }
    }
}
