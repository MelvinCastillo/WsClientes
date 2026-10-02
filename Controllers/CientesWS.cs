namespace WsClientes.Controllers
{
    using Microsoft.EntityFrameworkCore;

    namespace WsSofterpRD.Model
    {
        [PrimaryKey(nameof(Idcodigo))]
        public class ClientesWS
        {
            public string Idcodigo { get; set; } = null!;

            public string Nombre1 { get; set; } = null!;

            public string Nombre2 { get; set; } = null!;

            public string? Apellido1 { get; set; }

            public string? Apellido2 { get; set; }

            public DateTime Fechacreacion { get; set; }

            public string iduser { get; set; } = null!;
        }
    }
}
