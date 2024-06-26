﻿namespace Common.Requests;

public class PriceUpdate {

    public int sellerId { get; set; }

    public int productId { get; set; }

    public float price { get; set; }

    public string version { get; set; }

    public string instanceId { get; set; }

    public PriceUpdate(){ }

}