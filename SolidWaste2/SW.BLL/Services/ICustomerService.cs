using SW.DM;

namespace SW.BLL.Services;

public interface ICustomerService
{
    Task Add(Customer customer, string addToi);
    Task Add(Customer customer);
    Task<ICollection<Customer>> GetAll();
    Task<Customer> GetById(int id);
    Task<Customer> GetByPE(int peId);
    Task<int> GetNextCustomerNumber(string customerType);
    Task Update(Customer customer);
}
