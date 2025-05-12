import { clusterApiUrl, Connection } from '@solana/web3.js';
import { ConfigService } from '@nestjs/config';
import * as dotenv from 'dotenv';

dotenv.config();

const configService = new ConfigService();

const network = configService.get<string>('SOLANA_NETWORK');

export const connection = new Connection(network!, 'confirmed');
