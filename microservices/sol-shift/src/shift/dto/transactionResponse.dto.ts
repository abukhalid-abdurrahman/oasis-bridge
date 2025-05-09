import { ApiProperty } from '@nestjs/swagger';

class TransactionData {
  @ApiProperty({ description: 'Base64-encoded transaction', example: 'AbC123==' })
  base64: string;
}

export class TransactionResponseDto {
  @ApiProperty({ example: 'success', enum: ['success', 'error'] })
  status: string;

  @ApiProperty({ example: 'Transaction created successfully.' })
  message: string;

  @ApiProperty({ example: 200 })
  code: number;

  @ApiProperty({ type: TransactionData, required: false })
  data?: TransactionData;
}
