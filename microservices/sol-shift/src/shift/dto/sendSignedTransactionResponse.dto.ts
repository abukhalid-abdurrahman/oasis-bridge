import { ApiProperty } from "@nestjs/swagger";
import { BaseResponseDto } from "src/common/dto/baseResponse.dto";

class TransactionId {
  @ApiProperty({ description: 'Transaction ID', example: 'AbC123==' })
  transactionId: string;
}

export class SendSignedTransactionResponseDto extends BaseResponseDto {
  @ApiProperty({ type: TransactionId })
  data?: TransactionId
}