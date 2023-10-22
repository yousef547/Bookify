using Bookify.Web.Core.Consts;
using Bookify.Web.Core.ViewModel;
using Bookify.Web.Core.ViewModels;
using System.ComponentModel.DataAnnotations;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModel
{
    public class RentalReturnFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Penalty Paid?")]
        [AssertThat("(TotalDelayInDays == 0 && PenaltyPaid == false) || PenaltyPaid == true", ErrorMessage = Errors.PenaltyShouldBePaid)]
        public bool PenaltyPaid { get; set; }

        public IList<RentalCopyViewModel> Copies { get; set; } = new List<RentalCopyViewModel>();

        public List<ReturnCopyViewModel> SelectedCopies { get; set; } = new();

        public bool AllowExtend { get; set; }

        public int TotalDelayInDays 
        { 
            get
            {
                return Copies.Sum(c => c.DelayInDays);
            }
        }
    }
}