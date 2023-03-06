using System;
using System.Collections.Generic;

namespace FiveMinuteCoder
{
    public class CartDataModel
    {
        public string CartId {get;set;}
        public string UserId {get;set;}
        public List<ProductDataModel> Products {get;set;}
    }

}