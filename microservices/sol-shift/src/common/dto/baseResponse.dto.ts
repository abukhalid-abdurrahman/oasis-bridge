import { ApiProperty } from '@nestjs/swagger';

export class BaseResponseDto {
  @ApiProperty({ example: 'success', enum: ['success', 'error'] })
  status: string;

  @ApiProperty({ example: 'Transaction created successfully.' })
  message: string;

  @ApiProperty({ example: 200 })
  code: number;
}
