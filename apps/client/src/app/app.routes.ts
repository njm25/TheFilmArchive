import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { FilmComponent } from './pages/film/film.component';
import { SourceComponent } from './pages/source/source.component';

export const routes: Routes = [
    {
        path: "",
        component: HomeComponent
    },
    {
        path: "film/:id",
        component: FilmComponent
    },
    {
        path: "source/:id",
        component: SourceComponent
    }

];
