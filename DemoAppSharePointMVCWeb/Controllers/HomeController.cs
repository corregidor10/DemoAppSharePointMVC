using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DemoAppSharePointMVCWeb.Models;

namespace DemoAppSharePointMVCWeb.Controllers
{
    public class HomeController : Controller
    {
        [SharePointContextFilter]
        public ActionResult Index()
        {
            User spUser = null;

            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    spUser = clientContext.Web.CurrentUser;

                    clientContext.Load(spUser, user => user.Title);

                    clientContext.ExecuteQuery();

                    ViewBag.UserName = spUser.Title;
                }
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult TotalPedidos()
        {
            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    ListCollection listas = web.Lists;
                    clientContext.Load<ListCollection>(listas);
                    clientContext.ExecuteQuery();

                    var pedidos = listas.GetByTitle("Pedidos");
                    clientContext.Load(pedidos);
                    var productos = listas.GetByTitle("Productos");
                    clientContext.Load(productos);
                    clientContext.ExecuteQuery();
                    CamlQuery pedidosQuery = new CamlQuery();
                    ListItemCollection pedidosItems = pedidos.GetItems(pedidosQuery);
                    clientContext.Load(pedidosItems);
                    clientContext.ExecuteQuery();


                    var total = 0.0;
                    var clientes = new Dictionary<string, double>();


                    foreach (var item in pedidosItems)
                    {
                        FieldLookupValue lookup = item["Producto"] as FieldLookupValue;
                        int lId = lookup.LookupId;
                        var uds = item["Unidades"];
                        var pi = productos.GetItemById(lId);
                        clientContext.Load(pi);
                        clientContext.ExecuteQuery();
                        var precio = pi["Precio"];
                        var venta = (double)precio * (double)uds;
                        total += venta;

                        if (clientes.ContainsKey(item["Title"].ToString()))
                        {
                            clientes[item["Title"].ToString()] = clientes[item["Title"].ToString()] +
                                                                        venta;

                        }
                        else
                        {
                            clientes.Add(item["Title"].ToString(), venta);
                        }

                    }

                    var mc = total / clientes.Keys.Count;

                    var model = new Totales() { Numero = pedidosItems.Count, MediaCliente = mc, Total = total };

                    return View(model);


                }
                return View();

            }
        }

        public ActionResult ListaPedidos()
        {
            List<Pedidos> model = new List<Pedidos>();
            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    ListCollection listas = web.Lists;
                    clientContext.Load<ListCollection>(listas);
                    clientContext.ExecuteQuery();

                    var pedidos = listas.GetByTitle("Pedidos");
                    clientContext.Load(pedidos);
                    var productos = listas.GetByTitle("Productos");
                    clientContext.Load(productos);
                    clientContext.ExecuteQuery();

                    CamlQuery pedidosQuery = new CamlQuery();
                    ListItemCollection pedidosItems = pedidos.GetItems(pedidosQuery);
                    clientContext.Load(pedidosItems);
                    clientContext.ExecuteQuery();

                    foreach (var item in pedidosItems)
                    {
                        FieldLookupValue lookup = item["Producto"] as FieldLookupValue;
                        int lkId = lookup.LookupId;
                        int uds;
                        int.TryParse(item["Unidades"].ToString(), out uds);
                        var pi = productos.GetItemById(lkId);
                        clientContext.Load(pi);
                        clientContext.ExecuteQuery();

                        var precio = pi["Precio"];
                        var venta = (double)precio * (double)uds;

                        model.Add(new Pedidos()
                        {
                            Cliente = item["Title"].ToString(),
                            Pedido = pi["Title"].ToString(),
                            Unidades = uds,
                            Total = venta


                        });
                    }


                }
                return View(model);

            }

        }

        public ActionResult Add()
        {
            var prodList = new List<Productos>();
            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    ListCollection listas = web.Lists;
                    clientContext.Load<ListCollection>(listas);
                    clientContext.ExecuteQuery();

                    var productos = listas.GetByTitle("Productos");
                    clientContext.Load(productos);
                    clientContext.ExecuteQuery();
                    CamlQuery productosQuery= new CamlQuery();

                    ListItemCollection productosItems = productos.GetItems(productosQuery);
                    clientContext.Load(productosItems);
                    clientContext.ExecuteQuery();

                    foreach (var item in productosItems)
                    {
                        int id;
                        int.TryParse(item["ID"].ToString(), out id);

                        prodList.Add(new Productos()
                        {
                            Id = id,
                            Nombre = item["Title"].ToString()

                        });
                    }
                }
            }
            ViewBag.idProducto=new SelectList(prodList, "Id", "Nombre");
            return View(new Pedidos());
        }

        [HttpPost]
        public ActionResult Add(Pedidos model)
        {
            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    ListCollection listas = web.Lists;
                    clientContext.Load<ListCollection>(listas);
                    clientContext.ExecuteQuery();

                    var pedidos = listas.GetByTitle("Pedidos");
                    clientContext.Load(pedidos);
                   
                    ListItemCreationInformation listCreationInformation= new ListItemCreationInformation();

                    ListItem oListItem = pedidos.AddItem(listCreationInformation);

                    oListItem["Title"] = model.Cliente;
                    oListItem["Unidades"] = model.Unidades;
                    oListItem["Fecha"] = DateTime.Now;
                    var lv= new FieldLookupValue() {LookupId = model.idProducto};
                    oListItem["Producto"] = lv;
                    oListItem.Update();
                    clientContext.ExecuteQuery();
                }
                return RedirectToAction("Index",
                    new { SPHostUrl = SharePointContext.GetSPHostUrl(HttpContext.Request).AbsoluteUri });
            }
        }
    }
}

