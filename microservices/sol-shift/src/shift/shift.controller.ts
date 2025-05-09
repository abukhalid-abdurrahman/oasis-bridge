import { Body, Controller, Post } from '@nestjs/common';
import { ApiResponse, ApiTags } from '@nestjs/swagger';
import { ShiftService } from './shift.service';
import { CreateTransactionDto } from './dto/createTransaction.dto';
import { TransactionResponseDto } from './dto/transactionResponse.dto';
import { SendSignedTransactionDto } from './dto/sendSignedTransaction.dt';
import { SendSignedTransactionResponseDto } from './dto/sendSignedTransactionResponse.dto';

@ApiTags('Shift')
@Controller('shift')
export class ShiftController {
  constructor(private readonly shiftService: ShiftService) {}

  @Post('create-transaction')
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

  @Post('send-transaction')
  sendSignedTransaction(
    @Body() dto: SendSignedTransactionDto,
  ): Promise<SendSignedTransactionResponseDto> {
    return this.shiftService.sendSignedTransaction(dto);
  }
}
