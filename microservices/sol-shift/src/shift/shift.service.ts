import {
  Keypair,
  PublicKey,
  Transaction,
  SystemProgram,
  LAMPORTS_PER_SOL,
  TransactionInstruction,
  SendTransactionError,
} from '@solana/web3.js';
import {
  createTransferInstruction,
  getOrCreateAssociatedTokenAccount,
} from '@solana/spl-token';
import {
  BadRequestException,
  HttpStatus,
  Injectable,
  InternalServerErrorException,
} from '@nestjs/common';
import { connection } from 'src/solana/connection';
import bs58 from 'bs58';
import { CreateTransactionDto } from './dto/createTransaction.dto';
import { TransactionResponseDto } from './dto/transactionResponse.dto';
import { SendSignedTransactionDto } from './dto/sendSignedTransaction.dt';
import { SPL_LAMPROTS } from 'src/lib/constants';

@Injectable()
export class ShiftService {
  async createPurchaseTransaction(
    dto: CreateTransactionDto,
  ): Promise<TransactionResponseDto> {
    try {
      const {
        buyerPubkey,
        sellerPubkey,
        sellerSecretkey,
        nftMint,
        price,
        tokenMint,
      } = dto;

      const buyer = new PublicKey(buyerPubkey);
      const seller = new PublicKey(sellerPubkey);
      const nft = new PublicKey(nftMint);
      const escrow = Keypair.fromSecretKey(bs58.decode(sellerSecretkey));

      const transaction = new Transaction();
      const blockhash = await connection.getLatestBlockhash();
      transaction.recentBlockhash = blockhash.blockhash;
      transaction.feePayer = buyer;
      transaction.add(
        new TransactionInstruction({
          keys: [],
          programId: new PublicKey(
            'MemoSq4gqABAXKb96qnH8TysNcWxMyWCqXgDLGmfcHr',
          ),
          data: Buffer.from(`purchase:${buyerPubkey}:${nftMint}`),
        }),
      );

      if (tokenMint) {
        const token = new PublicKey(tokenMint);

        const buyerToken = await getOrCreateAssociatedTokenAccount(
          connection,
          escrow,
          token,
          buyer,
        );

        const sellerToken = await getOrCreateAssociatedTokenAccount(
          connection,
          escrow,
          token,
          seller,
        );

        transaction.add(
          createTransferInstruction(
            buyerToken.address,
            sellerToken.address,
            buyer,
            price * SPL_LAMPROTS,
          ),
        );
      } else {
        transaction.add(
          SystemProgram.transfer({
            fromPubkey: buyer,
            toPubkey: seller,
            lamports: price * LAMPORTS_PER_SOL,
          }),
        );
      }

      const escrowNft = await getOrCreateAssociatedTokenAccount(
        connection,
        escrow,
        nft,
        escrow.publicKey,
      );

      const buyerNft = await getOrCreateAssociatedTokenAccount(
        connection,
        escrow,
        nft,
        buyer,
      );

      transaction.add(
        createTransferInstruction(
          escrowNft.address,
          buyerNft.address,
          escrow.publicKey,
          1,
        ),
      );

      transaction.partialSign(escrow);

      const serialized = transaction.serialize({ requireAllSignatures: false });

      return {
        status: 'success',
        message: 'Transaction created successfully.',
        code: HttpStatus.OK,
        data: {
          transaction: serialized.toString('base64'),
        },
      };
    } catch (error) {
      if (error.message) {
        throw new BadRequestException({
          status: 'error',
          message: error.message,
          code: HttpStatus.BAD_REQUEST,
        });
      }

      throw new InternalServerErrorException({
        status: 'error',
        message: 'Transaction creation failed.',
        code: HttpStatus.INTERNAL_SERVER_ERROR,
      });
    }
  }

  async sendSignedTransaction(dto: SendSignedTransactionDto) {
    try {
      const buffer = Buffer.from(dto.signedTransaction, 'base64');
      const transactionId = await connection.sendRawTransaction(buffer);

      return {
        status: 'success',
        message: 'Transaction sent successfully.',
        code: HttpStatus.OK,
        data: {
          transactionId,
        },
      };
    } catch (error: any) {
      if (
        error instanceof SendTransactionError &&
        error.message?.includes('already been processed')
      ) {
        const decodedTx = Transaction.from(
          Buffer.from(dto.signedTransaction, 'base64'),
        );
        const signatureBuffer = decodedTx.signatures[0]?.signature;
        const txId = signatureBuffer ? bs58.encode(signatureBuffer) : 'unknown';

        return {
          status: 'warning',
          message: 'Transaction was already processed.',
          code: HttpStatus.OK,
          data: {
            transactionId: txId,
          },
        };
      }

      throw new BadRequestException({
        status: 'error',
        message: error.message,
        code: HttpStatus.BAD_REQUEST,
      });
    }
  }
}
