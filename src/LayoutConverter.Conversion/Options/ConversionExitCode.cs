
namespace LayoutConverter.Conversion.Options;

public enum ConversionExitCode
{
    Success = 0,
    UnexpectedFailure = 1,
    InvalidArguments = 2,
    UnknownFileType = 3,
    InputAccessFailure = 4,
    NotYetImplemented = 5,
}
