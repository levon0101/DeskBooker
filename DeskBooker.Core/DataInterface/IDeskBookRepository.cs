using DeskBooker.Core.Domain;

namespace DeskBooker.Core.DataInterface
{
    public interface IDeskBookRepository
    {
        void Save(DeskBooking deskBooking);
    }
}
