namespace Sonovate.CodeTest
{
	using Domain;

	internal interface ICandidateRepository
	{
		Candidate GetById(string supplierId);
	}
}