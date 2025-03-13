namespace BEAUTIFY_QUERY.DOMAIN.Exceptions;
public static class DoctorCertificateException
{
    public class DoctorCertificateNotFoundException : NotFoundException
    {
        public DoctorCertificateNotFoundException(Guid doctorCertificateId)
            : base($"The doctor certificate with the id {doctorCertificateId} was not found.")
        {
        }
    }

    public class DoctorCertificateFieldException : NotFoundException
    {
        public DoctorCertificateFieldException(string doctorCertificateField)
            : base($"The doctor certificate with the field {doctorCertificateField} is not correct.")
        {
        }
    }
}