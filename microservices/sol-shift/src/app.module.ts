import { Module } from '@nestjs/common';
import { ShiftModule } from './shift/shift.module';
import { ConfigModule } from '@nestjs/config';

@Module({
  imports: [ConfigModule.forRoot({ isGlobal: true }), ShiftModule],
})
export class AppModule {}
