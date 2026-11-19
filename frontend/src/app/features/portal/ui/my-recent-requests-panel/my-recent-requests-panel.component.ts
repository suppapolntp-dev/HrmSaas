import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StatusIndicatorComponent } from '../../../../shared/ui/status-indicator/status-indicator.component';
import { StatusTone } from '../../../../shared/ui/status-indicator/status-indicator.component';
import {
  MyDashboardRequestItemDto,
  formatUtcDateTime,
  mapApprovalStatusLabel,
  mapApprovalStatusTone,
  mapRequestTypeLabel,
  parseApprovalPayloadSummary,
} from '../../models/portal-dashboard.models';

@Component({
  selector: 'app-my-recent-requests-panel',
  standalone: true,
  imports: [RouterLink, StatusIndicatorComponent],
  templateUrl: './my-recent-requests-panel.component.html',
  styleUrl: './my-recent-requests-panel.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyRecentRequestsPanelComponent {
  readonly requests = input.required<MyDashboardRequestItemDto[]>();

  protected getTypeLabel(requestType: string): string {
    return mapRequestTypeLabel(requestType);
  }

  protected getStatusLabel(status: string): string {
    return mapApprovalStatusLabel(status);
  }

  protected getStatusTone(status: string): StatusTone {
    return mapApprovalStatusTone(status);
  }

  protected getSummary(payloadSummary: string): string {
    return parseApprovalPayloadSummary(payloadSummary);
  }

  protected formatSubmittedAt(isoText: string): string {
    return formatUtcDateTime(isoText);
  }
}
