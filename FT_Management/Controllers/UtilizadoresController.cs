namespace FT_Management.Controllers
{
    public class UtilizadoresController : Controller
    {
        private readonly ILogger<UtilizadoresController> _logger;

        public UtilizadoresController(ILogger<UtilizadoresController> logger)
        {
            _logger = logger;
        }

        //Obter todos os utilizadores
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter uma listagem de acesso de todos os utilizadores.", u.NomeCompleto, u.Id);

            return View(context.ObterListaUtilizadores(false, false));
        }

        //Obtem view para login
        [HttpGet]
        public IActionResult Login(string ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        //Efetua o login do utilizador com 2FA
        [HttpPost]
        public async Task<IActionResult> Login(Utilizador utilizador, string ReturnUrl, int first, int second, int third, int fourth, int fifth, int sixth)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            List<Utilizador> LstUtilizadores = context.ObterListaUtilizadores(true, false).Where(u => u.NomeUtilizador == utilizador.NomeUtilizador).ToList();

            if (LstUtilizadores.Count == 0)
            {
                Cliente c = phccontext.ObterClienteNIF(utilizador.NomeUtilizador);
                if (c.IdCliente != 0)
                {
                    if (c.Senha == utilizador.Password)
                    {
                        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development") return Forbid();
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, c.NumeroContribuinteCliente),
                            new Claim(ClaimTypes.GivenName, c.NomeCliente),
                            new Claim(ClaimTypes.Role, "Cliente")
                        };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        context.AdicionarLog(c.IdCliente, "Cliente login com sucesso!", 4);
                        return RedirectToAction("Adicionar", "RMA");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Password errada!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Não foram encontrados utlizadores com esse nome!");
                }
            }

            foreach (var user in LstUtilizadores)
            {
                var passwordHasher = new PasswordHasher<string>();
                if (passwordHasher.VerifyHashedPassword(null, user.Password, utilizador.Password) == PasswordVerificationResult.Success)
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" && !user.Dev) return Forbid();

                    _logger.LogDebug("Utilizador {1} [{2}] a realizar um login.", user.NomeCompleto, user.Id);

                    TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                    string res = first > 9 ? first.ToString() : first.ToString() + second.ToString() + third.ToString() + fourth.ToString() + fifth.ToString() + sixth.ToString();
                    if (string.IsNullOrEmpty(user.SecondFactorAuthStamp) || tfa.ValidateTwoFactorPIN(user.SecondFactorAuthStamp, res))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Id.ToString()),
                            new Claim(ClaimTypes.GivenName, user.NomeCompleto),
                            new Claim(ClaimTypes.Thumbprint, user.ImgUtilizador),
                            new Claim(ClaimTypes.Role, user.Id == 1 ? "Master" : ""),
                            new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User"),
                            new Claim(ClaimTypes.Role, user.TipoUtilizador == 1 ? "Tech" : user.TipoUtilizador == 2 ? "Comercial" : "Escritorio"),
                            new Claim(ClaimTypes.UserData, user.TipoMapa == 1 ? "Google Maps" : (user.TipoMapa == 2 ? "Waze" : "Apple"))

                        };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        context.AdicionarLog(user.Id, "Utilizador realizou um login com sucesso!", 4);

                        if (ReturnUrl != "" && ReturnUrl != null)
                        {
                            Response.Redirect(ReturnUrl, true);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "2FA errado!");
                        context.AdicionarLog(user.Id, "Utilizador tentou um login sem sucesso!", 4);
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Password errada!");
                    context.AdicionarLog(user.Id, "Utilizador tentou um login sem sucesso!", 4);
                }
            }
            return View();
        }

        //Obtem um utilizador em especifico
        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        [HttpGet]
        public IActionResult Utilizador(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter informação de um utilizador em especifico: {3}", u.NomeCompleto, u.Id, id);

            if (id == 0) id = int.Parse(this.User.Claims.First().Value.ToString());
            if (!User.IsInRole("Admin") && id != int.Parse(this.User.Claims.First().Value)) return RedirectToAction("Editar", new { id = int.Parse(this.User.Claims.First().Value) });

            List<Zona> LstZonas = context.ObterZonas();
            LstZonas.Insert(0, new Zona() { Id = 0, Valor = "N/D" });
            ViewBag.Zonas = LstZonas.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Valor });

            List<TipoTecnico> LstTipoTecnicos = context.ObterTipoTecnicos();
            LstTipoTecnicos.Insert(0, new TipoTecnico() { Id = 0, Valor = "N/D" });
            ViewBag.TipoTecnico = LstTipoTecnicos.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Valor });

            List<KeyValuePair<string, string>> LstChats = ChatContext.ObterChatsAtivos();
            LstChats.Insert(0, new KeyValuePair<string, string>("", "N/D"));
            ViewBag.Chats = LstChats.Select(l => new SelectListItem() { Value = l.Key, Text = l.Value });

            List<KeyValuePair<int, string>> LstNotificacoes = new List<KeyValuePair<int, string>>() { new KeyValuePair<int, string>(0, "Desativado"), new KeyValuePair<int, string>(1, "Email"), new KeyValuePair<int, string>(2, "Nextcloud"), new KeyValuePair<int, string>(3, "Ambos") };
            ViewBag.Notificacoes = LstNotificacoes.Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });

            Utilizador t = context.ObterUtilizador(id);
#if !DEBUG
                if (string.IsNullOrEmpty(t.SecondFactorAuthStamp))
                {
                    String stamp = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);

                    TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                    SetupCode setupInfo = tfa.GenerateSetupCode("FoodTech", t.NomeUtilizador, stamp, false, 3);

                    t.SecondFactorImgUrl = setupInfo.QrCodeSetupImageUrl;
                    t.SecondFactorAuthCode = setupInfo.ManualEntryKey;
                    ViewData["2FASTAMP"] = stamp;
                }
#endif
            return View(t);
        }

        //Atualiza um utilizador em especifico
        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        [HttpPut]
        public ContentResult Utilizador(int id, Utilizador utilizador, int enable, int acessos, int dev, int admin, int api)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador t = context.ObterUtilizador(id);
            if ((t.Admin & !this.User.IsInRole("Master")) && t.Id != int.Parse(this.User.Claims.First().Value.ToString())) return Content("");
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a atualizar a informação de um utilizador em especifico: Id - {3}, Enabled - {4}, Acessos - {5}, Dev - {6}, Admin - {7}, Api - {8}, .", u.NomeCompleto, u.Id, id, enable, acessos, dev, admin, api);

            if (utilizador.TipoMapa > 0)
            {
                t.Pin = utilizador.Pin == null ? "" : utilizador.Pin;
                t.Iniciais = utilizador.Iniciais == null ? "" : utilizador.Iniciais;
                t.CorCalendario = utilizador.CorCalendario == null ? "" : utilizador.CorCalendario;
                t.TipoMapa = utilizador.TipoMapa;
                t.Telemovel = utilizador.Telemovel == null ? "" : utilizador.Telemovel;
                t.DataNascimento = utilizador.DataNascimento;
                t.TipoTecnico = utilizador.TipoTecnico;
                t.Zona = utilizador.Zona;
                t.ChatToken = utilizador.ChatToken == null ? "" : utilizador.ChatToken;
                t.NotificacaoAutomatica = utilizador.NotificacaoAutomatica;
            }

            if (enable > 0) t.Enable = enable == 1;
            if (acessos > 0) t.Acessos = acessos == 1;
            if (dev > 0) t.Dev = dev == 1;
            if (admin > 0) t.Admin = admin == 1;

            if (api == 1) return Content(context.NovaApiKey(t));

            context.NovoUtilizador(t);
            if (!string.IsNullOrEmpty(t.ChatToken)) ChatContext.EnviarNotificacao("Foram atualizadas as suas informações de utilizador!", t);

            return Content("1");
        }

        //Apaga um utilizador em especifico
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public ContentResult Utilizador(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador t = context.ObterUtilizador(int.Parse(id));
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a apagar a informação de um utilizador em especifico: {3}.", u.NomeCompleto, u.Id, id);

            if (!t.Admin || this.User.IsInRole("Master")) context.ApagarUtilizador(t);

            return Content("1");
        }

        //Atualizar senha do utilizador
        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        [HttpPut]
        public ContentResult Senha(int id, string password_current, string password, string password_confirmation)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador t = context.ObterUtilizador(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a alterar a senha de um utilizador em especifico: {3}.", u.NomeCompleto, u.Id, id);

            string res = "";

            if (password != password_confirmation) res += "Passwords não condizem\r\n";
            if (password.Length < 8) res += "Password demasiado pequena! Tem de ter pelo menos 8 digitos!\r\n";
            if (!password.Any(char.IsUpper)) res += "Tem de ter pelo menos uma letra maiscula!\r\n";
            if (!password.Any(char.IsNumber)) res += "Tem de ter pelo menos um número\r\n";

            if (res == "")
            {
                var passwordHasher = new PasswordHasher<string>();
                if (passwordHasher.VerifyHashedPassword(null, u.Password, password_current) == PasswordVerificationResult.Success)
                {
                    t.Password = passwordHasher.HashPassword(null, password);
                    context.NovoUtilizador(t);
                }
                else
                {
                    res += "Password atual incorreta!\r\n";
                }
            }

            return Content(res);
        }

        //Remove a senha do utilizador e coloca outra
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public IActionResult Senha(int id, string senha)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var passwordHasher = new PasswordHasher<string>();
            if (string.IsNullOrEmpty(senha)) return Content("Nok");
            Utilizador t = context.ObterUtilizador(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a eliminar a senha de um utilizador em especifico: {3}.", u.NomeCompleto, u.Id, id);

            if (!t.Admin || this.User.IsInRole("Master"))
            {
                t.Password = passwordHasher.HashPassword(null, senha);
                t.SecondFactorAuthStamp = "";
            }

            context.NovoUtilizador(t);

            return Content("Ok");
        }

        //Obtem o 2FA
        [HttpGet]
        public bool SecondFA(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return !string.IsNullOrEmpty(context.ObterListaUtilizadores(true, false).Where(u => u.NomeUtilizador == id).DefaultIfEmpty(new Utilizador()).First().SecondFactorAuthStamp);
        }

        //Atualiza o 2FA
        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        [HttpPost]
        public IActionResult SecondFA(int id, string code, string stamp)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador t = context.ObterUtilizador(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a atualizar o 2FA de um utilizador em especifico: {3}.", u.NomeCompleto, u.Id, id);

            t.SecondFactorAuthStamp = "";

            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            if (tfa.ValidateTwoFactorPIN(stamp, code))
            {
                t.SecondFactorAuthStamp = stamp;
                if (!string.IsNullOrEmpty(t.ChatToken)) ChatContext.EnviarNotificacao("Foram atualizadas as suas informações de utilizador!", u);
                context.NovoUtilizador(t);
            }

            return RedirectToAction("Editar", new { id = t.Id });
        }

        //Remove o 2 FA
        [HttpDelete]
        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        public IActionResult SecondFA(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador t = context.ObterUtilizador(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a remover o 2FA de um utilizador em especifico: {3}.", u.NomeCompleto, u.Id, id);

            t.SecondFactorAuthStamp = "";

            if (!string.IsNullOrEmpty(t.ChatToken)) ChatContext.EnviarNotificacao("Foram atualizadas as suas informações de utilizador!", u);
            context.NovoUtilizador(t);

            return RedirectToAction("Editar", new { id = t.Id });
        }

        //Atualiza a imagem do utilizador
        [HttpPost]
        public IActionResult Imagem(int id, IFormFile file)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador t = context.ObterUtilizador(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a atualizar a imagem de um utilizador em especifico: {3}.", u.NomeCompleto, u.Id, id);


            if (file.Length > 0)
            {
                FicheirosContext.CriarImagemUtilizador(file, t.NomeUtilizador);
            }

            t.ImgUtilizador = "/img/" + t.NomeUtilizador + "/" + file.FileName;

            context.NovoUtilizador(t);
            FicheirosContext.GestaoFicheiros(true, false);

            return RedirectToAction("Logout");
        }

        //Obtem as permissoes todas
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public JsonResult Permissoes()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(context.ObterPermissoes());
        }

        //Atualiza as permissoes do user
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public IActionResult Permissoes(int id, string[] perms)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a atualizar as permissoes de um utilizador em especifico: {3}.", u.NomeCompleto, u.Id, id);

            return Content("0");
        }

        //Obtem os logs do utilziador numa data em especifico
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Logs(int id, string Data)
        {
            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["Data"] = Data;

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] obter os logs de um utilizador em especifico: {3}.", u.NomeCompleto, u.Id, id);

            ViewData["NomeUtilizador"] = context.ObterUtilizador(id).NomeUtilizador;

            return View(context.ObterListaLogs(id).Where(l => l.Data > DateTime.Parse(Data) && l.Data < DateTime.Parse(Data).AddDays(1)));
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
