namespace Estetica_Unicid.Models
{
    public class AgendamentoModel : BaseModel
    {
        public ClienteModel Cliente { get; set; }
        public Guid ClienteId { get; set; }
        public ServicoModel Servico { get; set; }
        public Guid ServicoId { get; set; }
    }
}