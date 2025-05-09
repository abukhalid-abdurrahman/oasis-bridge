import { ApiProperty } from '@nestjs/swagger';
import { BaseResponseDto } from 'src/common/dto/baseResponse.dto';

class TransactionData {
  @ApiProperty({ description: 'Base64-encoded transaction', example: 'AbC123==' })
  transaction: string;
}

export class TransactionResponseDto extends BaseResponseDto {
  @ApiProperty({ type: TransactionData, required: false })
  data?: TransactionData;
}
