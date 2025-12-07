using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetAPI.Dtos
{
    public class PostToAddDto
    {
        public string PostTitle { get; set; }
        public string PostContent { get; set; }

        public PostToAddDto()
        {
            if (PostTitle == null)
                PostTitle = "";

            if (PostContent == null)
                PostContent = "";
        }
    }
}
