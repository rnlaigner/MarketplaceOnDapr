﻿namespace Common.Entities
{
    /**
     * Product is based on info found in:
     * (i) https://dev.olist.com/docs/creating-a-product
     * (ii) Olist data set, order_items file
     * It is worthy to note that the attributes gtin, stock, package mesasures, photo, and tags are not considered
     * Besides, only one category is chosen as found in olist public data set
     */
    public class Product
	{
        public long seller_id { get; set; }

        public long product_id { get; set; }

        public string name { get; set; } = "";

        public string sku { get; set; } = "";

        public string category_name { get; set; } = "";

        public string description { get; set; } = "";

        public decimal price { get; set; }

        public decimal freight_value { get; set; }

        // "2017-10-06T01:40:58.172415Z"
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public bool active { get; set; }

        // https://dev.olist.com/docs/products
        // approved by default
        public string status { get; set; } = "";

    }
}

