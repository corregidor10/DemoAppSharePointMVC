using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoAppSharePointMVCWeb.Models
{
    public class Pedidos
    {
        public String Cliente { get; set; }
        public String Pedido { get; set; }
        public int Unidades { get; set; }
        public double Total { get; set; }
        public int idProducto { get; set; }

    }
}