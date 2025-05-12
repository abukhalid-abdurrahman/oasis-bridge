import { ApiProperty } from "@nestjs/swagger";
import { IsNotEmpty, IsNumber, IsOptional, IsString } from "class-validator";

export class CreateTransactionDto {
  @IsNotEmpty()
  @IsString()
  @ApiProperty()
  readonly buyerPubkey: string;

  @IsNotEmpty()
  @IsString()
  @ApiProperty()
  readonly sellerPubkey: string;

  @IsNotEmpty()
  @IsString()
  @ApiProperty()
  readonly sellerSecretkey: string;

  @IsNotEmpty()
  @IsString()
  @ApiProperty()
  readonly nftMint: string;
  
  @IsNotEmpty()
  @IsNumber()
  @ApiProperty()
  readonly price: number;

  @IsOptional()
  @IsString()
  @ApiProperty({ required: false })
  readonly tokenMint: string;
}
