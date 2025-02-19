using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Product
    {
        [Required(ErrorMessage = "商品名称不能为空")]
        [StringLength(20, ErrorMessage = "商品名称长度不能超过20个字符")]
        public string Name { get; set; }

        //[Required(ErrorMessage = "价格不能为空")]
        //[Range(1, int.MaxValue, ErrorMessage = "价格必须是有效的整数")]
        public string Price { get; set; }

    }
}
