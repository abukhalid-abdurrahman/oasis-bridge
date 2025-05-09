import { ApiProperty } from "@nestjs/swagger";
import { IsNotEmpty, IsString } from "class-validator";

export class SendSignedTransactionDto {
  @IsNotEmpty()
  @IsString()
  @ApiProperty({ description: 'Base64-encoded signed transaction', example: 'AbC123==' })
  signedTransaction: string;
}
