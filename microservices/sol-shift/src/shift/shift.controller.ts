import { Body, Controller, Post } from '@nestjs/common';
import { ApiResponse, ApiTags } from '@nestjs/swagger';
import { ShiftService } from './shift.service';
import { CreateTransactionDto } from './dto/createTransaction.dto';
import { TransactionResponseDto } from './dto/transactionResponse.dto';

@ApiTags('Shift')
@Controller('shift')
export class ShiftController {
  constructor(private readonly shiftService: ShiftService) {}

  @Post('create-tx')
  @ApiResponse({
    status: 200,
    description: 'Transaction successfully created',
    type: TransactionResponseDto,
  })
  createTransaction(
    @Body() dto: CreateTransactionDto,
  ): Promise<TransactionResponseDto> {
    return this.shiftService.createPurchaseTransaction(dto);
  }
}
