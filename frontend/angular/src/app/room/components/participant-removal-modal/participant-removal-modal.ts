import { Component, input, output } from '@angular/core';

import { CommonModalTemplate } from '../../../shared/components/modal/common-modal-template/common-modal-template';
import {
  ButtonText,
  ModalSubtitle,
  ModalTitle,
  PictureName,
} from '../../../app.enum';

@Component({
  selector: 'app-participant-removal-modal',
  imports: [CommonModalTemplate],
  templateUrl: './participant-removal-modal.html',
})
export class ParticipantRemovalModal {
  readonly participantName = input.required<string>();
  readonly confirm = output<void>();
  readonly cancel = output<void>();

  public readonly pictureName = PictureName.Cookie;
  public readonly title = ModalTitle.ParticipantRemoval;
  public readonly buttonText = ButtonText.Confirm;
  public readonly subtitle = ModalSubtitle.ParticipantRemoval;

  public onConfirmClick(): void {
    this.confirm.emit();
  }

  public onCancelClick(): void {
    this.cancel.emit();
  }
}