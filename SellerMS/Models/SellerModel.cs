﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellerMS.Models
{

    [Table("sellers", Schema = "seller")]
    [PrimaryKey(nameof(id))]
    public class SellerModel
	{

        public int id { get; set; }

        public string name { get; set; } = "";

        public string company_name { get; set; } = "";

        public string email { get; set; } = "";

        public string phone { get; set; } = "";

        public string mobile_phone { get; set; } = "";

        public string cpf { get; set; } = "";

        public string cnpj { get; set; } = "";

        public string address { get; set; } = "";

        public string complement { get; set; } = "";

        public string city { get; set; } = "";

        public string state { get; set; } = "";

        public string zip_code { get; set; } = "";

        public SellerModel()
		{
		}
	}
}

