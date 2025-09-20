using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RupalStudentCore8App.Server.Models.Auth;

namespace RupalStudentCore8App.Server.Models
{
    public class ParentsViewModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string IdNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string IdType { get; set; }
        [Column(TypeName = "date")]
        public DateTime IdIssuanceDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime IdExpiryDate { get; set; }
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        [Required]
        [StringLength(100)]
        public string FullNameAr { get; set; }
        [Required]
        public int NationalityId { get; set; }
        [Required]
        [StringLength(50)]
        public string Relation { get; set; }
        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(200)]
        public string Email { get; set; }
        [Required]
        [StringLength(200)]
        public string EmploymentPlace { get; set; }
        [Required]
        [StringLength(100)]
        public string Occupation { get; set; }
        [Required]
        [StringLength(300)]
        public string Address { get; set; }
        public bool IsPrimaryParent { get; set; }
    }

    public class ChildrenViewModel
    {
        [Required]
        [StringLength(50)]
        public string IdNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string IdType { get; set; }
        [Required]
        [StringLength(200)]
        public string FullName { get; set; }
        [Required]
        [StringLength(200)]
        public string FullNameAr { get; set; }
        [Required]
        public int NationalityId { get; set; }
        [Column(TypeName = "date")]
        public DateTime IdIssuanceDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime IdExpiryDate { get; set; }
        [Required]
        [StringLength(200)]
        public string PlaceOfBirth { get; set; }
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [StringLength(500)]
        public string Address { get; set; }
        [Required]
        public int? AcademicYear { get; set; }
        [Required]
        public int? SchoolLevel { get; set; }
        [Required]
        public int? Grade { get; set; }
        [Required]
        public int? Section { get; set; }
        [Required]
        public string Status { get; set; }
        public IFormFile ImageUrl { get; set; }
    }

    public class ContactViewModel
    {
        [StringLength(50)]
        public string IdNumber { get; set; }
        [StringLength(200)]
        public string FullName { get; set; }
        [StringLength(200)]
        public string FullNameAr { get; set; }
        public int? NationalityId { get; set; }
        [StringLength(50)]
        public string Relation { get; set; }
        [StringLength(50)]
        public string PhoneNumber { get; set; }
        [StringLength(200)]
        public string Email { get; set; }
    }

    public class RegistrationViewModel
    {
        [StringLength(50)]
        public string IdNumber { get; set; }
        [StringLength(50)]
        public string IdType { get; set; }
        public DateTime IdIssuanceDate { get; set; }
        public DateTime IdExpiryDate { get; set; }
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        [Required]
        [StringLength(100)]
        public string FullNameAr { get; set; }
        public int NationalityId { get; set; }
        [Required]
        [StringLength(50)]
        public string Relation { get; set; }
        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(200)]
        public string Email { get; set; }
        [StringLength(200)]
        public string EmploymentPlace { get; set; }
        [StringLength(100)]
        public string Occupation { get; set; }
        [StringLength(300)]
        public string Address { get; set; }
        public bool IsPrimaryParent { get; set; }
        public int? ParentRegFormId { get; set; }
    }

    public class AddContractViewModel
    {
        public int StudentId { get; set; }
        public int? SchoolLevelId { get; set; }
        public int? GradeId { get; set; }
        public int? SectionId { get; set; }
        public int? AcademicYearId { get; set; }
        public int? ParentRegFormId { get; set; }
        public int? StudentContractId { get; set; }

    }

    public class PaymentFilter : PageModel
    {
        public string PaymentStatus { get; set; }
    }
    public class CustomerDetails
    {
        public string Email { get; set; }
        public CustomerName Name { get; set; }
        public CustomerAddress Address { get; set; }
        public string Phone { get; set; }
    }
    public class CustomerAddress
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string AreaCode { get; set; }
    }

    public class CustomerName
    {
        public string Title { get; set; }
        public string Forenames { get; set; }
        public string Surname { get; set; }
    }

    public class CreateOrderRequest
    {
        public string Method { get; set; }
        public int Store { get; set; }
        public string AuthKey { get; set; }
        public int Framed { get; set; }
        public CustomerDetails Customer { get; set; }
        public CreateOrder Order { get; set; }
        public Return Return { get; set; }
    }
  
    public class CreateOrder
    {
        public string CartId { get; set; }
        public string Test { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
    }

    public class Return
    {
        public string Authorised { get; set; }
        public string Declined { get; set; }
        public string Cancelled { get; set; }
    }

    public class CheckOrderRequest
    {
        public string Method { get; set; }
        public int Store { get; set; }
        public string AuthKey { get; set; }
        public int Framed { get; set; }
        public CheckOrder Order { get; set; }
        public Return Return { get; set; }
    }

    public class CheckOrder
    {
        public string Ref { get; set; }
    }

    public class CreateOrderResponse
    {
        public string Method { get; set; }
        public CreateOrderOject Order { get; set; }
        public ErrorObject Error { get; set; }
    }

    public class CreateOrderOject
    {
        public string Ref { get; set; }
        public string Url { get; set; }
    }

    public class CheckOrderResponse
    {
        public string Method { get; set; }
        public string Trace { get; set; }
        public CheckOrderObject Order { get; set; }
        public ErrorObject Error { get; set; }
    }

    public class CheckOrderObject
    {
        public string Ref { get; set; }
        public string Url { get; set; }
        public string CartId { get; set; }
        public string Test { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public StatusResponse Status { get; set; }
    }

    public class StatusResponse
    {
        public int Code { get; set; }
        public string Text { get; set; }
    }

    public class ErrorObject
    {
        public string Message { get; set; }
        public string Note { get; set; }
    }

}
