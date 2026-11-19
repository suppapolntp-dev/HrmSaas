import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import {
  ApprovalRequestDto,
  formatUtcDateTime,
  mapRequestTypeLabel,
  parseApprovalPayloadSummary,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-pending-approvals-panel',
  standalone: true,
  imports: [RouterLink, StatusIndicatorComponent],
  templateUrl: './pending-approvals-panel.component.html',
  styleUrl: './pending-approvals-panel.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PendingApprovalsPanelComponent {
  readonly approvals = input.required<readonly ApprovalRequestDto[]>();
  readonly submittingRequestId = input<string | null>(null);

  readonly approveClick = output<ApprovalRequestDto>();
  readonly rejectClick = output<ApprovalRequestDto>();

  protected getRequestTypeLabel(requestType: string): string {
    return mapRequestTypeLabel(requestType);
  }

  protected formatSubmittedAt(isoText: string): string {
    return formatUtcDateTime(isoText);
  }

  protected getSummaryText(payloadSummary: string): string {
    return parseApprovalPayloadSummary(payloadSummary);
  }

  protected onApprove(item: ApprovalRequestDto): void {
    if (this.submittingRequestId() !== null) {
      return;
    }
    this.approveClick.emit(item);
  }

  protected onReject(item: ApprovalRequestDto): void {
    if (this.submittingRequestId() !== null) {
      return;
    }
    this.rejectClick.emit(item);
  }
}
