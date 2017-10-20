//
//  MenuTarefasModel.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using System.Collections.Generic;
using SPromoterMobile.Models;

namespace SPromoterMobile
{
    [Preserve(AllMembers =true)] 
	public class MenuTarefasModel
	{
		public List<ListTypePDV> idVisitas;
		public MenuTarefasDA db;
		public List<string> idsUsuariosLogados;
		public FormDinamicoCon formDinamico;
	}
}

