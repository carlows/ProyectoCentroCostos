﻿using CentroCostos.Infrastructure;
using CentroCostos.Infrastructure.Repositorios;
using CentroCostos.Models;
using CentroCostos.Models.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CentroCostos.Controllers
{
    public class AdministracionController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly ILineaRepository _lineasDb;
        private readonly IModeloRepository _modelosDb;
        private readonly IMaterialRepository _materialesDb;
        private readonly ICategoriaRepository _categoriasDb;
        private readonly ICostoRepository _costosDb;
        private readonly IDepartamentoRepository _departamentosDb;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public AdministracionController(IUnitOfWork uow, ILineaRepository lineasRepository,
            IModeloRepository modelosRepository, IMaterialRepository materialRepository, ICategoriaRepository categoriaRepository,
            ICostoRepository costosRepository, IDepartamentoRepository departamentosRepository)
        {
            _uow = uow;
            _lineasDb = lineasRepository;
            _modelosDb = modelosRepository;
            _materialesDb = materialRepository;
            _categoriasDb = categoriaRepository;
            _costosDb = costosRepository;
            _departamentosDb = departamentosRepository;
        }

        // GET: Administracion
        public ActionResult Index()
        {
            return View();
        }

        // GET: LineasProduccion
        public ActionResult LineasProduccion()
        {
            var model = new LineasProduccionAdmViewModel()
            {
                Lineas = _lineasDb.FindAll()
            };

            return View(model);
        }

        // GET: NuevaLinea
        public ActionResult NuevaLinea()
        {
            return View();
        }

        // POST: NuevaLinea
        [HttpPost]
        public ActionResult NuevaLinea(NuevaLineaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var nuevaLinea = new Linea
                {
                    Nombre_Linea = model.Codigo
                };

                try
                {
                    _lineasDb.Create(nuevaLinea);
                    _uow.SaveChanges();

                    TempData["message"] = "La linea ha sido creada correctamente";

                    return RedirectToAction("LineasProduccion");
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error al crear una linea nueva");
                    ModelState.AddModelError("", "Se produjo un error al intentar crear la linea nueva");
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        // GET: EditarLinea
        public ActionResult EditarLinea(int id)
        {
            var linea = _lineasDb.GetById(id);

            if(linea == null)
            {
                TempData["message_error"] = "No se pudo encontrar el registro especificado";
                return RedirectToAction("LineasProduccion");
            }

            var model = new NuevaLineaViewModel
            {
                Id = linea.Id,
                Codigo = linea.Nombre_Linea
            };

            return View(model);
        }

        // POST: EditarLinea
        [HttpPost]
        public ActionResult EditarLinea(NuevaLineaViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var linea = _lineasDb.GetById(model.Id);

                    linea.Nombre_Linea = model.Codigo;

                    _lineasDb.Update(linea);
                    _uow.SaveChanges();

                    TempData["message"] = "La linea fue modificada correctamente";
                    return RedirectToAction("ModelosLinea", new { id = model.Id });
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error al editar linea");
                    ModelState.AddModelError("", "Se produjo un error al intentar modificar esta linea");
                }
            }

            return View(model);
        }

        // GET: ModelosLinea
        public ActionResult ModelosLinea(int id)
        {
            var linea = _lineasDb.GetById(id);

            if(linea == null)
            {
                TempData["message_error"] = "El registro especificado no se pudo encontrar";
                return RedirectToAction("LineasProduccion");
            }

            var model = new ModelosLineaViewModel
            {
                Linea = linea,
                Modelos = linea.Modelos_Linea
            };

            return View(model);
        }

        // GET: NuevoModelo
        public ActionResult NuevoModelo(int id)
        {
            var model = new NuevoModeloViewModel
            {
                IdLinea = id
            };

            return View(model);
        }

        // POST: NuevoModelo
        [HttpPost]
        public ActionResult NuevoModelo(NuevoModeloViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var linea = _lineasDb.GetById(model.IdLinea);

                    var modeloNuevo = new Modelo
                    {
                        Codigo = model.Codigo,
                        Horma = model.Horma,
                        Planta = model.Planta,
                        Tipo_Suela = model.Tipo_Suela,
                        Numeracion = model.Numeracion,
                        Pieza = model.Pieza,
                        Color = model.Color,
                        Linea = linea
                    };

                    modeloNuevo.URL_Imagen = CheckAndUploadImage(model);

                    _modelosDb.Create(modeloNuevo);
                    _uow.SaveChanges();

                    TempData["message"] = "El modelo fue creado correctamente";
                    return RedirectToAction("ModelosLinea", new { id = model.IdLinea });
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error al agregar nuevo modelo");
                    ModelState.AddModelError("", "Se produjo un error al agregar el modelo");
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        // GET: EditarModelo
        public ActionResult EditarModelo(int idLinea, int id)
        {
            var linea = _lineasDb.GetById(idLinea);
            var modelo = _modelosDb.GetById(id);

            if (modelo == null || linea == null)
            {
                logger.Warn("No se pudo encontrar el modelo o la linea con id {0}", id);
                TempData["message_error"] = "No se pudo encontrar el registro especificado";
                return RedirectToAction("ModelosLinea", new { id = idLinea });
            }

            var viewModel = new NuevoModeloViewModel
            {
                IdLinea = idLinea,
                id = modelo.Id,
                Horma = modelo.Horma,
                Tipo_Suela = modelo.Tipo_Suela,
                Codigo = modelo.Codigo,
                Color = modelo.Color,
                Numeracion = modelo.Numeracion,
                Pieza = modelo.Pieza,
                Planta = modelo.Planta
            };

            ViewBag.URLImagen = modelo.URL_Imagen;

            return View(viewModel);
        }

        // POST: EditarModelo
        [HttpPost]
        public ActionResult EditarModelo(NuevoModeloViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var curModel = _modelosDb.GetById(model.id);
                    var linea = _lineasDb.GetById(model.IdLinea);

                    curModel.Horma = model.Horma;
                    curModel.Codigo = model.Codigo;
                    curModel.Color = model.Color;
                    curModel.Numeracion = model.Numeracion;
                    curModel.Pieza = model.Pieza;
                    curModel.Planta = model.Planta;
                    curModel.Tipo_Suela = model.Tipo_Suela;
                    curModel.Linea = linea;
                    curModel.Fecha_Ultima_Modificacion = DateTime.Now;

                    curModel.URL_Imagen = CheckAndUploadImage(model) ?? curModel.URL_Imagen;

                    _modelosDb.Update(curModel);
                    _uow.SaveChanges();

                    TempData["message"] = "Registro modificado correctamente";
                    return RedirectToAction("ModelosLinea", new { id = model.IdLinea });
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error al editar modelo");

                    ModelState.AddModelError("", "Se produjo un error al editar el modelo");
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        // GET: Materiales
        public ActionResult Materiales()
        {
            var model = new MaterialesAdmViewModel
            {
                Materiales = _materialesDb.FindAll(),
                Categorias = _categoriasDb.FindAll()
            };

            return View(model);
        }

        // GET: NuevoMaterial
        public ActionResult NuevoMaterial()
        {
            var model = new MaterialViewModel
            {
                Categorias = _categoriasDb.FindAll()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Categoria
                })
            };

            return View(model);
        }

        // POST: NuevoMaterial
        [HttpPost]
        public ActionResult NuevoMaterial(MaterialViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var categoriaMaterial = _categoriasDb.GetById(model.CategoriaId);
                    var costoMaterial = new Costo
                    {
                        esCostoDirecto = true,
                        Comentario = model.Descripcion_Material,
                        Descripcion = "Material"
                    };

                    var material = new Material
                    {
                        Codigo = model.Codigo,
                        Descripcion_Material = model.Descripcion_Material,
                        Costo_Unitario = model.Costo_Unitario,
                        Consumo_Par = model.Consumo_Par,
                        Unidad_Medida = model.Unidad_Medida,
                        Categoria_Material = categoriaMaterial,
                        Costo = costoMaterial
                    };

                    _materialesDb.Create(material);
                    _uow.SaveChanges();
                    TempData["message"] = "El material se agregó correctamente";

                    return RedirectToAction("Materiales");
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error al agregar material");
                    ModelState.AddModelError("", "Se produjo un error al intentar agregar este material");
                    return View(model);
                }
            }

            model.Categorias = _categoriasDb.FindAll()
                                .Select(c => new SelectListItem
                                {
                                    Value = c.Id.ToString(),
                                    Text = c.Categoria
                                });
            return View(model);
        }

        // GET: EditarMaterial
        public ActionResult EditarMaterial(int id)
        {
            var material = _materialesDb.GetById(id);

            if(material == null)
            {
                TempData["message_error"] = "No se pudo encontrar el registro especificado";
                return RedirectToAction("Materiales");
            }

            var model = new MaterialViewModel
            {
                Id = material.Id,
                CategoriaId = material.Categoria_Material.Id,
                Codigo = material.Codigo,
                Descripcion_Material = material.Descripcion_Material,
                Costo_Unitario = material.Costo_Unitario,
                Unidad_Medida = material.Unidad_Medida,
                Categorias = _categoriasDb.FindAll()
                                .Select(c => new SelectListItem
                                {
                                    Value = c.Id.ToString(),
                                    Text = c.Categoria
                                })
            };

            return View(model);
        }

        // POST: EditarMaterial
        [HttpPost]
        public ActionResult EditarMaterial(MaterialViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var material = _materialesDb.GetById(model.Id);
                    var categoria = _categoriasDb.GetById(model.CategoriaId);

                    material.Codigo = model.Codigo;
                    material.Costo_Unitario = model.Costo_Unitario;
                    material.Unidad_Medida = model.Unidad_Medida;
                    material.Descripcion_Material = model.Descripcion_Material;
                    material.Categoria_Material = categoria;
                    material.Costo.Descripcion = model.Descripcion_Material;

                    _materialesDb.Update(material);
                    _uow.SaveChanges();

                    TempData["message"] = "Registro modificado correctamente";
                    return RedirectToAction("Materiales");
                }
                catch(Exception e)
                {
                    logger.Error(e, "Se produjo un error al editar un material");
                    ModelState.AddModelError("", "Se produjo un error al intentar editar el material");
                    return View(model);
                }
            }

            model.Categorias = _categoriasDb.FindAll()
                                .Select(c => new SelectListItem
                                {
                                    Value = c.Id.ToString(),
                                    Text = c.Categoria
                                });
            return View(model);
        }

        // GET: NuevaCategoria
        public ActionResult NuevaCategoria()
        {
            return View();
        }

        // POST: NuevaCategoria
        [HttpPost]
        public ActionResult NuevaCategoria(CategoriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                CategoriaMaterial categoria = new CategoriaMaterial
                {
                    Categoria = model.Categoria
                };

                try
                {
                    _categoriasDb.Create(categoria);
                    _uow.SaveChanges();

                    TempData["message"] = "Registro agregado correctamente";
                    return RedirectToAction("Materiales");
                }
                catch (Exception e)
                {
                    logger.Error(e, "Se produjo un error al agregar una categoria");
                    ModelState.AddModelError("", "Se produjo un error al intentar agregar la categoria");
                    return View(model);
                }
            }

            return View(model);
        }

        // GET: EditarCategoria
        public ActionResult EditarCategoria(int id)
        {
            var categoria = _categoriasDb.GetById(id);

            if(categoria == null)
            {
                TempData["message_error"] = "No se pudo encontrar el registro especificado";
                return RedirectToAction("Materiales");
            }

            var model = new CategoriaViewModel
            {
                Id = categoria.Id,
                Categoria = categoria.Categoria
            };

            return View(model);
        }

        // POST: EditarCategoria
        [HttpPost]
        public ActionResult EditarCategoria(CategoriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var categoria = _categoriasDb.GetById(model.Id);

                    categoria.Categoria = model.Categoria;

                    _categoriasDb.Update(categoria);
                    _uow.SaveChanges();

                    TempData["message"] = "Registro modificado correctamente";
                    return RedirectToAction("Materiales");
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error al editar una categoria");
                    ModelState.AddModelError("", "Se produjo un error al editar una categoria");
                }
            }

            return View(model);
        }

        // GET: Costos
        public ActionResult Costos()
        {
            var model = new CostosAdmViewModel
            {
                Costos = _costosDb.FindAll().Select(c => new CostoViewModel
                {
                    Id = c.Id,
                    Comentario = c.Comentario,
                    Descripcion = c.Descripcion,
                    Departamento = c.Departamento != null ? c.Departamento.Nombre_Departamento : "",
                    esCostoDirecto = c.esCostoDirecto
                }).ToList()
            };

            return View(model);
        }

        // GET: NuevoCosto
        public ActionResult NuevoCosto()
        {
            return View();
        }

        // POST: NuevoCosto
        [HttpPost]
        public ActionResult NuevoCosto(CostoViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var costo = new Costo
                    {
                        Descripcion = model.Descripcion,
                        Comentario = model.Comentario,
                        esCostoDirecto = (bool)model.esCostoDirecto
                    };

                    _costosDb.Create(costo);
                    _uow.SaveChanges();

                    TempData["message"] = "Registro agregado correctamente";
                    return RedirectToAction("Costos");
                }
                catch(Exception e)
                {
                    logger.Error(e, "Error al agregar un costo");
                    ModelState.AddModelError("", "Se produjo un error al intentar agregar este costo");
                }
            }

            return View(model);
        }

        // GET: EditarCosto
        public ActionResult EditarCosto(int id)
        {
            var costo = _costosDb.GetById(id);

            if(costo == null)
            {
                TempData["message_error"] = "No se pudo encontrar el registro especificado";
                return RedirectToAction("Costos");
            }

            var model = new CostoViewModel
            {
                Id = costo.Id,
                Descripcion = costo.Descripcion,
                Comentario = costo.Comentario,
                esCostoDirecto = (bool)costo.esCostoDirecto
            };

            return View(model);
        }

        // POST: EditarCosto
        [HttpPost]
        public ActionResult EditarCosto(CostoViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var costo = _costosDb.GetById(model.Id);

                    costo.Descripcion = model.Descripcion;
                    costo.Comentario = model.Comentario;
                    costo.esCostoDirecto = (bool)model.esCostoDirecto;                    

                    _costosDb.Update(costo);
                    _uow.SaveChanges();

                    TempData["message"] = "Registro modificado correctamente";
                    return RedirectToAction("Costos");
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error al editar un costo");
                    ModelState.AddModelError("", "Se produjo un error al intentar editar este costo");
                }
            }

            return View(model);
        }

        // GET: Departamentos
        public ActionResult Departamentos()
        {
            var model = new DepartamentosAdmViewModel
            {
                Departamentos = _departamentosDb.FindAll()
            };

            return View(model);
        }

        // GET: NuevoDepartamento
        public ActionResult NuevoDepartamento()
        {
            return View();
        }

        // POST: NuevoDepartamento
        [HttpPost]
        public ActionResult NuevoDepartamento(DepartamentoViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var departamento = new Departamento
                    {
                        Nombre_Departamento = model.Nombre_Departamento,
                        esDeProduccion = model.esDeProduccion
                    };

                    _departamentosDb.Create(departamento);
                    _uow.SaveChanges();

                    TempData["message"] = "El registro fue creado correctamente";
                    return RedirectToAction("Departamentos");
                }
                catch(Exception e)
                {
                    logger.Error(e, "Ocurrio un error al agregar un departamento");
                    ModelState.AddModelError(String.Empty, "Ocurrio un error al agregar el departamento");
                    return View(model);
                }
            }

            return View(model);
        }

        // GET: EditarDepartamento
        public ActionResult EditarDepartamento(int id)
        {
            var departamento = _departamentosDb.GetById(id);

            if(departamento == null)
            {
                TempData["message_error"] = "No se pudo encontrar el registro especificado";
                return RedirectToAction("Departamentos");
            }

            var model = new DepartamentoViewModel
            {
                Id = departamento.Id,
                Nombre_Departamento = departamento.Nombre_Departamento,
                esDeProduccion = departamento.esDeProduccion
            };

            return View(model);
        }

        // POST: EditarDepartamento
        [HttpPost]
        public ActionResult EditarDepartamento(DepartamentoViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var departamento = _departamentosDb.GetById(model.Id);

                    departamento.Nombre_Departamento = model.Nombre_Departamento;
                    departamento.esDeProduccion = model.esDeProduccion;

                    _departamentosDb.Update(departamento);
                    _uow.SaveChanges();

                    TempData["message"] = "El registro fue modificado correctamente";
                    return RedirectToAction("Departamentos");
                }
                catch(Exception e)
                {
                    logger.Error(e, "Error al editar departamento");
                    ModelState.AddModelError(String.Empty, "Ocurrio un error al intentar editar el departamento");
                    return View(model);
                }
            }

            return View(model);
        }

        private string CheckAndUploadImage(NuevoModeloViewModel model)
        {
            if (model.Imagen != null)
            {
                string serverPath = Server.MapPath("~/Content");
                return _modelosDb.UploadImage(model.Codigo, model.Imagen, serverPath);
            }

            return null;
        }
    }
}