using SPromoterMobile.Models;
using SPromoterMobile.Models.RESTful;

namespace SPromoterMobile
{
    [Preserve(AllMembers = true)]
    public class FormDinamicoModel
    {
        public FormDinamicoDA Db { get; set; }
        public FormSchemasRestModel.FormSchema CamposForm { get; set; }
        public string Id_form_server { get; set; }
        public string IdVisita { get; set; }
        public string IdProduto { get; set; }
        public int IdForm { get; set; }
        public int Tipo { get; set; }
    }
}

