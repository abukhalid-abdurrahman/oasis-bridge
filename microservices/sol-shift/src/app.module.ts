import { Module } from '@nestjs/common';
import { ShiftModule } from './shift/shift.module';

@Module({
  imports: [ShiftModule],
})
export class AppModule {}
