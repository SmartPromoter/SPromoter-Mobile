//
//  Preserve.cs
//
//  Author:
//       leonardcolusso <leonardcolusso@gmail.com>
//
//  Copyright (c) 2017 SmartPromoter
//
using System;
namespace SPromoterMobile.Models
{
    public sealed class PreserveAttribute : System.Attribute
    {
    	public bool AllMembers;
    	public bool Conditional;
    }
}
