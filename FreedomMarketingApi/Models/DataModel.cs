using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FreedomMarketingApi.Models
{
    public class DataModel
    {
        public class Roles
        {
            [Key]
            public int idRoles { get; set; }
            public string Description { get; set; }
            public string CreateDate { get; set; }
            public string RoleCode { get; set; }
            public string LastModifiedDate { get; set; }
        }
        public class Users
        {
            [Key]
            public int idUsers { get; set; }
            public string UserCode { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Identification { get; set; }
            public string Email { get; set; }
            public string Telephone { get; set; }
            public string Address { get; set; }
            public string Country { get; set; }
            public string ReferenceCode { get; set; }
            public int Points { get; set; }
            public string RoleCode { get; set; }
            public bool Status { get; set; }
            public string PaymentId { get; set; }
            public string CreateDate { get; set; }
            public string Password { get; set; }
            public bool MassiveMail { get; set; }
            public string LastModifiedDate { get; set; }
        }
        public class Payment
        {
            [Key]
            public int PaymentId { get; set; }
            public string PaymentSlip { get; set; }
            public string PaymentDate { get; set; }
            public string CreateDate { get; set; }
        }
        public class ResponseModel
        {
            public object HttpResponse { get; set; }
            public object FreedomResponse { get; set; }
        }
        public class LoginResponse
        {
            public string Email { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
        }
        public class LoginViewModel
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
        }
    }
}
