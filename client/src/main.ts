import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { registerLicense } from '@syncfusion/ej2-base';

import { environment } from './environments/environment';

registerLicense(environment.syncfusionLicense);

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));

