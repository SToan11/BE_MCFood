namespace ASM_C_5.DTOS.Responses
{
    public class BaseResponse<T>
    {
        public int ErrorCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
